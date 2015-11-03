using Data.Entities;
using Data.Interfaces;

namespace Hub.Managers.APIManagers.Packagers
{
    public interface IEmailPackager
    {
        void Send(IMailerDO envelope);
    }
}