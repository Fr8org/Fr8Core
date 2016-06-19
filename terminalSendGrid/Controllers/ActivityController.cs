using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;

namespace terminalSendGrid.Controllers
{
    public class ActivityController : DefaultActivityController
    {
        public ActivityController(IActivityExecutor activityExecutor)
            : base(activityExecutor)
        {
        }
    }
}