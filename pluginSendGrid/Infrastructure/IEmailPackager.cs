using Data.Entities;
using Data.Interfaces;

namespace pluginSendGrid.Infrastructure
{
    public interface IEmailPackager
    {
        void Send(IMailerDO envelope);
    }
}