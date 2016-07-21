using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;

namespace terminalDropbox.Controllers
{
    public class ActivityController : DefaultActivityController
    {
        public ActivityController(IActivityExecutor activityExecutor)
            : base(activityExecutor)
        {
        }
    }
}