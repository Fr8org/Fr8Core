using System.Net.Mail;

namespace Hub.Managers.InboundEmailHandlers
{
    interface IInboundEmailHandler
    {
        /// <summary>
        /// Tries to process passed email message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>If the message is of a wrong type returns false. Otherwise, if the message handled successfully, returns true.</returns>
        bool TryHandle(MailMessage message);
    }
}
