using System.Threading.Tasks;
using terminalUtilities.Models;

namespace terminalUtilities.Interfaces
{
    public interface IEmailPackager
    {
        Task Send(TerminalMailerDO mailer);
    }
}