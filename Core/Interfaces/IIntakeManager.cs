using System.Net.Mail;

namespace Core.Interfaces
{
    public interface IIntakeManager
    {
        void AddEmail(MailMessage mailMessage);
    }
}