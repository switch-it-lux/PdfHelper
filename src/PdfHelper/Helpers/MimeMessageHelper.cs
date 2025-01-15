using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using MimeKit;

namespace Sit.Pdf {

    internal static class MimeMessageHelper {

        public static async Task<MimeMessage> LoadOutlookMsgAsync(Stream msg, bool includeAttachments = true) {
            var mimeMessage = new MimeMessage();
            var msgMessage = new MsgReader.Outlook.Storage.Message(msg);
            mimeMessage.Subject = msgMessage.Subject;
            mimeMessage.Date = msgMessage.SentOn ?? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

            if (!string.IsNullOrWhiteSpace(msgMessage.Sender.Email))
                mimeMessage.From.Add(msgMessage.Sender.ToMimeKitMailboxAddress());

            foreach (var recipient in msgMessage.Recipients) {
                if (!string.IsNullOrWhiteSpace(recipient.Email)) {
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
                            break;
                        default:
                            mimeMessage.To.Add(recipient.ToMimeKitMailboxAddress());
                            break;
                    }
                }
            }

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(msgMessage.BodyHtml ?? $"<html><body style=\"font-family: Calibri\">{msgMessage.BodyText.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}<br/></body></html>");

            using (var sw = new StringWriter()) {
                html.Save(sw);
                var builder = new BodyBuilder {
                    HtmlBody = sw.ToString()
                };

                if (includeAttachments) {
                    foreach (var attachment in msgMessage.Attachments) {
                        if (attachment is MsgReader.Outlook.Storage.Attachment outlookAttachment) {
                            using (var messageStream = new MemoryStream(outlookAttachment.Data)) {
                                var mimeEntity = await builder.Attachments.AddAsync(outlookAttachment.FileName, messageStream);
                                mimeEntity.ContentId = outlookAttachment.ContentId;
                            }
                        } else if (attachment is MsgReader.Outlook.Storage.Message outlookMessage) {
                            using (var messageStream = new MemoryStream()) {
                                outlookMessage.Save(messageStream);
                                await builder.Attachments.AddAsync(outlookMessage.FileName, messageStream);
                            }
                        }
                    }
                }
                mimeMessage.Body = builder.ToMessageBody();
            }
            return mimeMessage;
        }

        public static async Task<Stream> ConvertToPdfAsync(MimeMessage mimeMessage, PageSize pageSize, bool includeAttachments = false) {
            var pdfStream = new MemoryStream();
            try {
                using (var writer = new PdfWriter(pdfStream)) {
                    writer.SetCloseStream(false);
                    using (var pdfDoc = new PdfDocument(writer)) {
                        //Setting page size
                        pdfDoc.SetDefaultPageSize(new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));

                        //Add attachments
                        if (includeAttachments && mimeMessage.Attachments.Any()) {
                            foreach (var attachment in mimeMessage.Attachments) {
                                using (var ms = new MemoryStream()) {
                                    if (attachment is MimePart part) {
                                        await part.Content.DecodeToAsync(ms);
                                        var fs = PdfFileSpec.CreateEmbeddedFileSpec(pdfDoc, ms.ToArray(), part.FileName, null);
                                        pdfDoc.AddFileAttachment(part.FileName, fs);
                                    } else if (attachment is MessagePart messagePart) {
                                        await messagePart.WriteToAsync(ms);
                                        var fs = PdfFileSpec.CreateEmbeddedFileSpec(pdfDoc, ms.ToArray(), messagePart.ContentDisposition.FileName, null);
                                        pdfDoc.AddFileAttachment(messagePart.ContentDisposition.FileName, fs);
                                    }
                                }
                            }
                        }

                        //Convert message to HTML and content to PDF
                        var html = await ConvertToHtmlAsync(mimeMessage, true);
                        var options = new ConverterProperties();
                        HtmlConverter.ConvertToPdf(html, pdfDoc, options);
                    }
                }
            } catch {
                pdfStream.Dispose();
                throw;
            }
            return pdfStream;
        }

        static async Task<string> ConvertToHtmlAsync(MimeMessage message, bool addEmailHeading = false) {
            //Main body message
            string s = message.HtmlBody ?? $"<p>{message.TextBody.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</p>";
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(s);

            //Adding email heading info
            if (addEmailHeading) {
                var heading = string.Empty;
                if (message.From.Mailboxes.Count() > 0) heading = $"<p><b>From: </b>{message.From.Mailboxes.ToString(true)}<br/>";
                if (message.To.Mailboxes.Count() > 0) heading += $"<b>To: </b>{message.To.Mailboxes.ToString(true)}<br/>";
                if (message.Cc.Mailboxes.Count() > 0) heading += $"<b>Cc: </b>{message.Cc.Mailboxes.ToString(true)}<br/>";
                if (message.Bcc.Mailboxes.Count() > 0) heading += $"<b>Bcc: </b>{message.Bcc.Mailboxes.ToString(true)}<br/>";
                if (message.Date.Year >= 1900) heading += $"<b>Sent: </b>{message.Date.ToLocalTime():f}<br/>";
                if (!string.IsNullOrWhiteSpace(message.Subject)) heading += $"<b>Subject: </b>{message.Subject}<br>";
                var attachmentNames = message.Attachments
                    .Where(x => !string.IsNullOrEmpty(x.ContentDisposition?.FileName))
                    .Select(x => x.ContentDisposition.FileName);
                if (attachmentNames.Count() > 0) heading += $"<b>Attachments: </b>{string.Join(", ", attachmentNames)}</p><hr>";

                if (!string.IsNullOrWhiteSpace(heading)) {
                    var node = html.DocumentNode.SelectSingleNode("//body");
                    if (node == null) node = html.DocumentNode.SelectSingleNode("//html");
                    if (node == null) node = html.DocumentNode;
                    node.InnerHtml = heading + " " + node.InnerHtml;
                }
            }

            //Handling embedded cid images
            var cidImages = html.DocumentNode.SelectNodes("//img[contains(@src, 'cid:')]");
            if (cidImages != null) {
                foreach (var cidImage in cidImages) {
                    var srcAttribute = cidImage.Attributes["src"];
                    var cidId = srcAttribute.Value.Substring("cid:".Length);
                    var bodyImg = message.BodyParts.FirstOrDefault(a => a.ContentId == cidId);
                    if (bodyImg != null) {
                        srcAttribute.Value = $"data:{bodyImg.ContentType.MimeType};base64,{Convert.ToBase64String(await bodyImg.ToByteArrayAsync())}";
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
