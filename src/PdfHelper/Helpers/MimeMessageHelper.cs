using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using MimeKit;

namespace Sitl.Pdf {

    internal static class MimeMessageHelper {
        // Regex are static + compiled for performance and consistency.
        // Matches empty src/href attributes (e.g., src="" or href= ) so they can be removed.
        private static readonly Regex EmptySrcOrHrefRegex = new Regex(
            @"\s(?:src|href)\s*=\s*(?:(['""])\s*\1|(?=[\s>]))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Matches CSS url() with empty/blank values to replace them with "none".
        private static readonly Regex EmptyUrlRegex = new Regex(
            @"url\(\s*(?:(['""])\s*\1)?\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Matches <a ... href="..."> tags so href values can be inspected and sanitized.
        private static readonly Regex AnchorHrefRegex = new Regex(
            @"<a\b([^>]*?)\bhref\s*=\s*(['""])(?<href>.*?)\2([^>]*?)>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        // Matches legacy CSS behavior:url(...) declarations to strip them out.
        private static readonly Regex BehaviorUrlRegex = new Regex(
            @"behavior\s*:\s*url\s*\([^)]*\)\s*;?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Matches CSS display:flex declarations to downgrade them to display:block.
        private static readonly Regex DisplayFlexRegex = new Regex(
            @"display\s*:\s*flex\s*;?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string AntiCollapseCss = "<style>body,.WordSection1,div{padding-top:0.05px;}</style>";

        public static async Task<MimeMessage> LoadOutlookMsgAsync(Stream msg, bool includeAttachments = true) {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            var mimeMessage = new MimeMessage();

            using (var msgMessage = new MsgReader.Outlook.Storage.Message(msg)) {
                mimeMessage.Subject = msgMessage.Subject;
                mimeMessage.Date = msgMessage.SentOn ?? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

                if (msgMessage.Sender != null && !string.IsNullOrWhiteSpace(msgMessage.Sender.Email))
                    mimeMessage.From.Add(msgMessage.Sender.ToMimeKitMailboxAddress());

                if (msgMessage.Recipients != null) {
                    foreach (var recipient in msgMessage.Recipients) {
                        if (recipient == null || string.IsNullOrWhiteSpace(recipient.Email))
                            continue;

                        switch (recipient.Type.Value) {
                            case MsgReader.Outlook.RecipientType.To:
                                mimeMessage.To.Add(recipient.ToMimeKitMailboxAddress());
                                break;
                            case MsgReader.Outlook.RecipientType.Cc:
                                mimeMessage.Cc.Add(recipient.ToMimeKitMailboxAddress());
                                break;
                            case MsgReader.Outlook.RecipientType.Bcc:
                                mimeMessage.Bcc.Add(recipient.ToMimeKitMailboxAddress());
                                break;
                            case MsgReader.Outlook.RecipientType.Resource:
                                // ignored
                                break;
                            default:
                                mimeMessage.To.Add(recipient.ToMimeKitMailboxAddress());
                                break;
                        }
                    }
                }

                var htmlDoc = new HtmlAgilityPack.HtmlDocument();

                // Fallback to BodyText if BodyHtml is missing.
                var bodyText = msgMessage.BodyText ?? string.Empty;
                var encodedBodyText = WebUtility.HtmlEncode(bodyText);
                var fallbackHtml = $"<html><body style=\"font-family: Calibri\">{encodedBodyText.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}<br/></body></html>";

                htmlDoc.LoadHtml(msgMessage.BodyHtml ?? fallbackHtml);

                using (var sw = new StringWriter()) {
                    htmlDoc.Save(sw);

                    var builder = new BodyBuilder {
                        HtmlBody = sw.ToString()
                    };

                    if (includeAttachments && msgMessage.Attachments != null) {
                        foreach (var attachment in msgMessage.Attachments) {
                            if (attachment is MsgReader.Outlook.Storage.Attachment outlookAttachment) {
                                using (var messageStream = new MemoryStream(outlookAttachment.Data)) {
                                    var fileName = string.IsNullOrWhiteSpace(outlookAttachment.FileName)
                                        ? "attachment"
                                        : outlookAttachment.FileName;

                                    var mimeEntity = await builder.Attachments.AddAsync(fileName, messageStream).ConfigureAwait(false);

                                    if (!string.IsNullOrWhiteSpace(outlookAttachment.ContentId))
                                        mimeEntity.ContentId = outlookAttachment.ContentId;
                                }
                            } else if (attachment is MsgReader.Outlook.Storage.Message outlookMessage) {
                                using (var messageStream = new MemoryStream()) {
                                    outlookMessage.Save(messageStream);
                                    messageStream.Position = 0;

                                    var fileName = string.IsNullOrWhiteSpace(outlookMessage.FileName)
                                        ? "attached-message.msg"
                                        : outlookMessage.FileName;

                                    await builder.Attachments.AddAsync(fileName, messageStream).ConfigureAwait(false);
                                }
                            }
                        }
                    }

                    mimeMessage.Body = builder.ToMessageBody();
                }
            }

            return mimeMessage;
        }

        public static async Task<Stream> ConvertToPdfAsync(MimeMessage mimeMessage, PageSize pageSize, bool includeAttachments = false) {
            if (mimeMessage == null) throw new ArgumentNullException(nameof(mimeMessage));

            var pdfStream = new MemoryStream();

            try {
                using (var writer = new PdfWriter(pdfStream)) {
                    writer.SetCloseStream(false);

                    using (var pdfDoc = new PdfDocument(writer)) {
                        // Setting page size
                        pdfDoc.SetDefaultPageSize(new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));

                        // Add attachments into PDF
                        if (includeAttachments && mimeMessage.Attachments != null && mimeMessage.Attachments.Any()) {
                            AddAttachmentsToPdf(pdfDoc, mimeMessage.Attachments);
                        }

                        var html = await ConvertToHtmlAsync(mimeMessage, true).ConfigureAwait(false);
                        html = SanitizeHtmlForPdf(html);

                        var options = new ConverterProperties();
                        options.SetBaseUri("file:///");

                        HtmlConverter.ConvertToPdf(html, pdfDoc, options);
                    }
                }

                pdfStream.Position = 0;
                return pdfStream;
            } catch {
                pdfStream.Dispose();
                throw;
            }
        }

        private static void AddAttachmentsToPdf(PdfDocument pdfDoc, IEnumerable<MimeEntity> attachments) {
            // We keep track of used attachment names because:
            // - PDF attachments are identified by name in the PDF catalog.
            // - Duplicate names can cause collisions or overwrite behavior depending on viewers/tools.
            // - Some emails contain multiple attachments with the same filename (common with "image001.png", etc.).
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Optimization:
            // Track the "next suffix index" per normalized filename key to avoid scanning (2..10000) each time.
            // This makes de-duplication typically O(1) even when many attachments share the same name.
            var nextIndexByKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var attachment in attachments) {
                using (var ms = new MemoryStream()) {
                    if (attachment is MimePart part) {
                        part.Content.DecodeTo(ms);

                        // Normalize the filename to:
                        // - ensure it's not null/empty,
                        // - ensure it doesn't contain invalid filesystem characters,
                        // - ensure uniqueness inside the PDF attachment list.
                        var attachmentName = NormalizeAttachmentName(part.FileName, "attachment", usedNames, nextIndexByKey);

                        // iText needs the full byte[] here to create the embedded file specification.
                        var fs = PdfFileSpec.CreateEmbeddedFileSpec(pdfDoc, ms.ToArray(), attachmentName, null);
                        pdfDoc.AddFileAttachment(attachmentName, fs);
                    } else if (attachment is MessagePart messagePart) {
                        messagePart.WriteTo(ms);

                        // MessagePart does NOT always expose FileName in older MimeKit versions.
                        // ContentDisposition.FileName is the usual place, but can be null as well.
                        var rawName = messagePart.ContentDisposition != null ? messagePart.ContentDisposition.FileName : null;
                        if (string.IsNullOrWhiteSpace(rawName))
                            rawName = "attached-message.eml";

                        var attachmentName = NormalizeAttachmentName(rawName, "attached-message.eml", usedNames, nextIndexByKey);

                        var fs = PdfFileSpec.CreateEmbeddedFileSpec(pdfDoc, ms.ToArray(), attachmentName, null);
                        pdfDoc.AddFileAttachment(attachmentName, fs);
                    } else {
                        // Unknown type => ignore safely.
                        // If you prefer strict behavior, you could throw here.
                    }
                }
            }
        }

        private static string NormalizeAttachmentName(
            string name,
            string fallback,
            HashSet<string> usedNames,
            Dictionary<string, int> nextIndexByKey) {

            // Ensure we always have a non-empty filename.
            // Some attachments have no filename (or only whitespace), which would make the PDF attachment list invalid or confusing.
            if (string.IsNullOrWhiteSpace(name))
                name = fallback;

            name = name.Trim();

            // Remove invalid filename characters in a single pass.
            // This avoids calling string.Replace repeatedly (once per invalid char), which can create many intermediate strings.
            name = ReplaceInvalidFileNameChars(name, '_');

            // If not used yet, we can keep the name as-is.
            if (usedNames.Add(name))
                return name;

            // At this point, a file with the same name already exists in the PDF attachment list.
            // We generate a unique name by adding " (2)", " (3)", ...
            // Instead of scanning from 2 every time, we keep a per-name counter for the next suffix to try.
            var baseName = System.IO.Path.GetFileNameWithoutExtension(name);
            var ext = System.IO.Path.GetExtension(name);

            // Key used to track suffix progression.
            // Using baseName+ext keeps behavior aligned with user-visible filename.
            var key = baseName + ext;

            int i;
            if (!nextIndexByKey.TryGetValue(key, out i)) {
                // First collision for that name => next candidate should be (2)
                i = 2;
            }

            while (true) {
                var candidate = $"{baseName} ({i}){ext}";
                if (usedNames.Add(candidate)) {
                    // Store next index to try for this base name.
                    nextIndexByKey[key] = i + 1;
                    return candidate;
                }

                // Candidate already used as well (e.g., email contains many duplicates).
                i++;

                // Safety: prevent pathological infinite loops if something is corrupted.
                // Still extremely unlikely to hit in real emails, but keeps the method total.
                if (i > 100000) {
                    return $"{Guid.NewGuid():N}{ext}";
                }
            }
        }

        private static string ReplaceInvalidFileNameChars(string input, char replacement) {
            // Build a lookup set for invalid chars.
            // Path.GetInvalidFileNameChars() is small, but HashSet avoids O(n*m) scans with repeated Replace calls.
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var invalidSet = new HashSet<char>(invalid);

            var sb = new StringBuilder(input.Length);
            foreach (var ch in input) {
                sb.Append(invalidSet.Contains(ch) ? replacement : ch);
            }
            return sb.ToString();
        }

        private static string SanitizeHtmlForPdf(string html) {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // 1) Remove empty src/href attributes like src="" or href=""
            // Some HTML-to-PDF converters may attempt to resolve them and fail or slow down.
            html = EmptySrcOrHrefRegex.Replace(html, "");

            // 2) Replace empty url() declarations in CSS with "none"
            // Empty url() can throw parsing errors in some HTML/CSS parsers.
            html = EmptyUrlRegex.Replace(html, "none");

            // 3) Disable the common broken hyperlink pattern href="http://"
            // This is not a valid URL and can trigger converter warnings or weird behavior.
            html = html.Replace("href=\"http://\"", "href=\"#\"");

            // 4) Neutralize non-absolute links (relative paths) by replacing them with "#"
            // Relative links can make the converter try to access local files or a base URI, which may fail.
            // We keep absolute URLs and anchors (#something) unchanged.
            html = AnchorHrefRegex.Replace(html, delegate (Match m) {
                var hrefGroup = m.Groups["href"];
                var href = hrefGroup != null ? (hrefGroup.Value ?? string.Empty).Trim() : string.Empty;

                Uri ignored;
                if (Uri.TryCreate(href, UriKind.Absolute, out ignored))
                    return m.Value;

                if (href.StartsWith("#"))
                    return m.Value;

                if (hrefGroup == null)
                    return m.Value;

                return m.Value.Replace($"href=\"{hrefGroup.Value}\"", "href=\"#\"");
            });

            // 5) Remove weird target attribute content
            // Some generated HTML contains target="''" which is invalid.
            html = html.Replace("target=\"''\"", "");

            // 6) Remove old IE-only CSS: behavior:url(...)
            // This is unsupported and can break CSS parsing.
            html = BehaviorUrlRegex.Replace(html, "");

            // 7) Inject a tiny padding CSS to prevent layout collapse (commonly seen in Word-generated HTML)
            // Some converters collapse top margins/padding, this acts as a workaround.
            html = InjectAntiCollapseCss(html);

            // 8) Replace display:flex with display:block
            // iText html2pdf has limited support for flex layout, and this avoids converter exceptions.
            html = DisplayFlexRegex.Replace(html, "display:block;");

            return html;
        }

        private static string InjectAntiCollapseCss(string html) {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // If there's a <head>, we inject the CSS inside it.
            // Otherwise we prepend it to the document.
            if (html.IndexOf("<head", StringComparison.OrdinalIgnoreCase) >= 0) {
                // Insert the anti-collapse CSS right after the opening <head ...> tag.
                return Regex.Replace(
                    html,
                    @"<head\b[^>]*>",
                    delegate (Match m) { return $"{m.Value}{AntiCollapseCss}"; },
                    RegexOptions.IgnoreCase);
            }

            return $"{AntiCollapseCss}{html}";
        }

        private static async Task<string> ConvertToHtmlAsync(MimeMessage message, bool addEmailHeading = false) {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // Main body message
            var textBody = message.TextBody ?? string.Empty;
            var encodedTextBody = WebUtility.HtmlEncode(textBody);
            var s = message.HtmlBody ?? $"<p>{encodedTextBody.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</p>";

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(s);

            // Adding email heading info
            if (addEmailHeading) {
                var heading = string.Empty;

                if (message.From.Mailboxes.Any())
                    heading = $"<p><b>From: </b>{WebUtility.HtmlEncode(message.From.Mailboxes.ToString(true))}<br/>";

                if (message.To.Mailboxes.Any())
                    heading += $"<b>To: </b>{WebUtility.HtmlEncode(message.To.Mailboxes.ToString(true))}<br/>";

                if (message.Cc.Mailboxes.Any())
                    heading += $"<b>Cc: </b>{WebUtility.HtmlEncode(message.Cc.Mailboxes.ToString(true))}<br/>";

                if (message.Bcc.Mailboxes.Any())
                    heading += $"<b>Bcc: </b>{WebUtility.HtmlEncode(message.Bcc.Mailboxes.ToString(true))}<br/>";

                if (message.Date.Year >= 1900)
                    heading += $"<b>Sent: </b>{WebUtility.HtmlEncode(message.Date.ToLocalTime().ToString("f"))}<br/>";

                if (!string.IsNullOrWhiteSpace(message.Subject))
                    heading += $"<b>Subject: </b>{WebUtility.HtmlEncode(message.Subject)}<br>";

                var attachmentNames = message.Attachments
                    .Select(x => x.ContentDisposition != null ? x.ContentDisposition.FileName : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => WebUtility.HtmlEncode(x))
                    .ToList();

                if (attachmentNames.Count > 0)
                    heading += $"<b>Attachments: </b>{string.Join(", ", attachmentNames)}</p><hr>";

                if (!string.IsNullOrWhiteSpace(heading)) {
                    var node = html.DocumentNode.SelectSingleNode("//body");
                    if (node == null) node = html.DocumentNode.SelectSingleNode("//html");
                    if (node == null) node = html.DocumentNode;

                    node.InnerHtml = $"{heading} {node.InnerHtml}";
                }
            }

            // Handling embedded cid images
            var cidImages = html.DocumentNode.SelectNodes("//img[@src]");
            if (cidImages != null) {
                foreach (var cidImage in cidImages) {
                    if (cidImage == null) continue;

                    var srcAttribute = cidImage.Attributes["src"];
                    if (srcAttribute == null) continue;

                    var srcValue = srcAttribute.Value ?? string.Empty;
                    if (!srcValue.StartsWith("cid:", StringComparison.OrdinalIgnoreCase)) continue;

                    var cidId = srcValue.Substring("cid:".Length).Trim().Trim('<', '>');
                    if (string.IsNullOrWhiteSpace(cidId)) continue;

                    var bodyImg = message.BodyParts.FirstOrDefault(a => {
                        var contentId = a.ContentId;
                        if (string.IsNullOrWhiteSpace(contentId)) return false;

                        contentId = contentId.Trim().Trim('<', '>');
                        return string.Equals(contentId, cidId, StringComparison.OrdinalIgnoreCase);
                    });
                    if (bodyImg != null) {
                        var bytes = await bodyImg.ToByteArrayAsync().ConfigureAwait(false);
                        srcAttribute.Value = $"data:{bodyImg.ContentType.MimeType};base64,{Convert.ToBase64String(bytes)}";
                    }
                }
            }

            using (var sw = new StringWriter()) {
                html.Save(sw);
                return sw.ToString();
            }
        }
    }
}
