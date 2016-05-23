using System.Threading.Tasks;

namespace terminalUtilities.Interfaces
{
    public interface IEmailPackager
    {
        Task Send(TerminalMailerDO mailer);
    }
}