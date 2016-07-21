using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;

namespace terminalSendGrid.Controllers
{
    public class TerminalController : DefaultTerminalController
    {
        public TerminalController(IActivityStore activityStore, IHubDiscoveryService hubDiscovery)
            : base(activityStore, hubDiscovery)
        {
        }
    }
}