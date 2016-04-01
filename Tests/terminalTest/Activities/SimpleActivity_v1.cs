using System.IO;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public class SimpleActivity_v1 : EnhancedTerminalActivity<SimpleActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
        }

        public SimpleActivity_v1() : base(false)
        {
        }

        protected override Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            return Task.FromResult(0);
        }

        protected override Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            File.AppendAllText(@"C:\Work\fr8_research\log.txt", $"{CurrentActivity.Label} started\n");
            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            File.AppendAllText(@"C:\Work\fr8_research\log.txt", $"{CurrentActivity.Label} ended\n");

            return Task.FromResult(0);
        }
    }
}