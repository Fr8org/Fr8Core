using Data.Entities;

namespace pluginSendGrid.Infrastructure
{
    public interface IEmailPackager
    {
        void Send(MailerDO envelope);
    }
}