using System;
using MimeKit;
using static MsgReader.Outlook.Storage;

namespace Sitl.Pdf {

    internal static class MsgReaderExtensions {

        private const string InvalidEmailAddress = "invalid@invalid";

        public static MailboxAddress ToMimeKitMailboxAddress(this Recipient recipient) {
            return CreateSafeMailboxAddress(recipient?.DisplayName, recipient?.Email);
        }

        public static MailboxAddress ToMimeKitMailboxAddress(this Sender sender) {
            return CreateSafeMailboxAddress(sender?.DisplayName, sender?.Email);
        }

        private static MailboxAddress CreateSafeMailboxAddress(string displayName, string emailAddress) {
            var safeDisplayName = displayName?.Trim().NullIfEmpty();
            var safeEmailAddress = string.IsNullOrWhiteSpace(emailAddress) ? InvalidEmailAddress : emailAddress.Trim();
            var mailboxName = safeDisplayName ?? safeEmailAddress;

            try {
                return new MailboxAddress(mailboxName, safeEmailAddress);
            } catch (ParseException) {
                return new MailboxAddress(mailboxName, InvalidEmailAddress);
            } catch (ArgumentException) {
                return new MailboxAddress(mailboxName, InvalidEmailAddress);
            }
        }
    }
}
