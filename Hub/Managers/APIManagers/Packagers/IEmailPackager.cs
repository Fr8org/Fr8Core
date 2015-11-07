using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Managers.APIManagers.Packagers
{
    public interface IEmailPackager
    {
        Task Send(IMailerDO envelope);
    }
}