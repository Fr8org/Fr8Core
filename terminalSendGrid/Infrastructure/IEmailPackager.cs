using System.Threading.Tasks;
namespace terminalSendGrid.Infrastructure
{
    public interface IEmailPackager
    {
        Task Send(TerminalMailerDO envelope);
    }
}