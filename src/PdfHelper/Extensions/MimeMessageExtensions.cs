using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;

namespace Sit.Pdf {

    internal static class MimeMessageExtensions {

        public static async Task<byte[]> ToByteArrayAsync(this MimeEntity mimeEntity) {
            using (var ms = new MemoryStream()) {
                if (mimeEntity is MessagePart rfc822)
                    await rfc822.Message.WriteToAsync(ms);
                else
                    await ((MimePart)mimeEntity).Content.DecodeToAsync(ms);

                return ms.ToArray();
            }
        }

        public static string ToString(this IEnumerable<MailboxAddress> addresses, bool includeDisplayName) {
            if (addresses == null) return null;
            return string.Join(", ", addresses.Select(a => {
                if (includeDisplayName && !string.IsNullOrWhiteSpace(a.Name) && a.Name != a.Address)
                    return a.Name + " &lt;" + a.Address + "&gt;";
                else
                    return "&lt;" + a.Address + "&gt;";
            }));
        }

        public static async Task<(byte[] Content, string Name)[]> GetAttachmentsAsFilesAsync(this MimeMessage message) {
            var res = new List<(byte[] Content, string Name)>();

            foreach (MimeEntity attachment in message.Attachments) {
                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                res.Add((await attachment.GetContentAsync(), fileName));
            }

            return res.ToArray();
        }

        static async Task<byte[]> GetContentAsync(this MimeEntity mimeEntity) {
            using (var ms = new MemoryStream()) {
                if (mimeEntity is MessagePart rfc822)
                    await rfc822.Message.WriteToAsync(ms);
                else
                    await ((MimePart)mimeEntity).Content.DecodeToAsync(ms);

                return ms.ToArray();
            }
        }
    }
}
