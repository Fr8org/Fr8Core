using Data.Entities;

namespace Core.Managers.APIManagers.Packagers
{
    public interface IEmailPackager
    {
        void Send(MailerDO envelope);
    }
}