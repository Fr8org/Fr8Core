using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace terminalUtilities.SendGrid
{
    public interface IEmailPackager
    {
        Task Send(IMailerDO envelope);
    }
}