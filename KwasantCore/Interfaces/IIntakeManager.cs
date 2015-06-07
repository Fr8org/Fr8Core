using System.Net.Mail;

namespace KwasantCore.Interfaces
{
    public interface IIntakeManager
    {
        void AddEmail(MailMessage mailMessage);
    }
}