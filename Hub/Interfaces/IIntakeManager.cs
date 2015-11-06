using System.Net.Mail;

namespace Hub.Interfaces
{
    public interface IIntakeManager
    {
        void AddEmail(MailMessage mailMessage);
    }
}