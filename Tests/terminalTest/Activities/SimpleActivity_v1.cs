using System.IO;
using System.Threading.Tasks;
using Data.Control;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public class SimpleActivity_v1 : EnhancedTerminalActivity<SimpleActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock TextBlock;
            public TextBox ActivityToAdd;
            public Button AddChild;

            public ActivityUi()
            {
                Controls.Add(TextBlock = new TextBlock());
                Controls.Add(ActivityToAdd = new TextBox { Label = "Activity to add" });
                Controls.Add(AddChild = new Button { Label = "Add child activity" });
                AddChild.Events.Add(ControlEvent.RequestConfigOnClick);
            }
        }

        public SimpleActivity_v1() : base(false)
        {
        }

        protected override Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.TextBlock.Value = CurrentActivity.Id.ToString();
            return Task.FromResult(0);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            if (ConfigurationControls.AddChild.Clicked)
            {
                ConfigurationControls.AddChild.Clicked = false;
                await AddAndConfigureChildActivity(CurrentActivity, ConfigurationControls.ActivityToAdd.Value, CurrentActivity.Label + "." + (CurrentActivity.ChildNodes.Count + 1), CurrentActivity.Label + "." + (CurrentActivity.ChildNodes.Count + 1));
            }
        }

        protected override Task RunCurrentActivity()
        {
            File.AppendAllText(@"C:\Work\fr8_research\log.txt", $"{CurrentActivity.Label} [{CurrentActivity.Id}] started\n");
            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            File.AppendAllText(@"C:\Work\fr8_research\log.txt", $"{CurrentActivity.Label} [{CurrentActivity.Id}] ended\n");

            return Task.FromResult(0);
        }
    }
}