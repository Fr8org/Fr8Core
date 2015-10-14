using Data.Entities;
using Data.Interfaces;

namespace Core.Managers.APIManagers.Packagers
{
    public interface IEmailPackager
    {
        void Send(IMailerDO envelope);
    }
}