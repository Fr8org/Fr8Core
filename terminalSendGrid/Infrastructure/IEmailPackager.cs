using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace terminalSendGrid.Infrastructure
{
    public interface IEmailPackager
    {
        Task Send(IMailerDO envelope);
    }
}