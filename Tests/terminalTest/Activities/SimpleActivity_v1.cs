using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public class SimpleActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
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
                var activityTemplate = await GetActivityTemplateByName(ConfigurationControls.ActivityToAdd.Value);
                await AddAndConfigureChildActivity(CurrentActivity, activityTemplate, CurrentActivity.Label + "." + (CurrentActivity.ChildNodes.Count + 1), CurrentActivity.Label + "." + (CurrentActivity.ChildNodes.Count + 1));
            }
        }

        protected override Task RunCurrentActivity()
        {
            Log($"{CurrentActivity.Label} [{CurrentActivity.Id}] started");
            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            Log($"{CurrentActivity.Label} [{CurrentActivity.Id}] ended");

            return Task.FromResult(0);
        }
    }
}