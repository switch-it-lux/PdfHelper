using static MsgReader.Outlook.Storage;

namespace Sit.Pdf {

    internal static class MsgReaderExtensions {

        public static MimeKit.MailboxAddress ToMimeKitMailboxAddress(this Recipient recipient) {
            return new MimeKit.MailboxAddress(recipient.DisplayName.NullIfEmpty() ?? recipient.Email, recipient.Email);
        }

        public static MimeKit.MailboxAddress ToMimeKitMailboxAddress(this Sender sender) {
            return new MimeKit.MailboxAddress(sender.DisplayName.NullIfEmpty() ?? sender.Email, sender.Email);
        }
    }
}
