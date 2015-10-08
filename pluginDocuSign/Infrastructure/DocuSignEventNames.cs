using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pluginDocuSign.Infrastructure
{
    public class DocuSignEventNames
    {
        public const int EnvelopeSent = 1;
        public const int EnvelopeDelivered = 2;
        public const int EnvelopeSigned = 3;
        public const int EnvelopeCompleted = 4;
        public const int EnvelopeDeclined = 5;
        public const int EnvelopeVoided = 6;

        public const int RecipientSent = 7;
        public const int RecipientDelivered = 8;
        public const int RecipientSigned = 9;
        public const int RecipientCompleted = 10;
        public const int RecipientDeclined = 11;
        public const int RecipientAuthenticationFailed = 12;
        public const int RecipientAutoResponded = 13;

        public static int MapEnvelopeExternalEventType(string status)
        {
            switch (status)
            {
                case "Sent":
                    return EnvelopeSent;
                case "Delivered":
                    return EnvelopeDelivered;
                case "Signed":
                    return EnvelopeSigned;
                case "Completed":
                    return EnvelopeCompleted;
                case "Declined":
                    return EnvelopeDeclined;
                case "Voided":
                    return EnvelopeVoided;
                default:
                    return 0;
            }
        }

        public static int MapRecipientExternalEventType(string status)
        {
            switch (status)
            {
                case "Sent":
                    return RecipientSent;
                case "Delivered":
                    return RecipientDelivered;
                case "Signed":
                    return RecipientSigned;
                case "Completed":
                    return RecipientCompleted;
                case "Declined":
                    return RecipientDeclined;
                case "AuthenticationFailed":
                    return RecipientAuthenticationFailed;
                case "AutoResponded":
                    return RecipientAutoResponded;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets all event names in the form of EnvelopeSent and RecipientSent
        /// </summary>
        public static string[] GetAllEventNames()
        {
            return typeof(DocuSignEventNames).GetFields().Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Gets events names in the form of Sent and Delivered for the given event type (Envelope or Recipient)
        /// </summary>
        public static string[] GetEventsFor(string eventType)
        {
            return
                GetAllEventNames()
                    .Where(name => name.StartsWith(eventType))
                    .Select(name => name.Remove(0, eventType.Length))
                    .ToArray();
        }

    }
}