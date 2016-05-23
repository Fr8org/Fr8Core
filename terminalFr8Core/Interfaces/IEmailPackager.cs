using System.Threading.Tasks;
using terminalFr8Core.Models;

namespace terminalFr8Core.Interfaces
{
    public interface IEmailPackager
    {
        Task Send(TerminalMailerDO mailer);
    }
}