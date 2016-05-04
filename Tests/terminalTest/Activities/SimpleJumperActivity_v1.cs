using System;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public class SimpleJumperActivity_v1 : TestActivityBase<SimpleJumperActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock TextBlock;
            public TextBox WhereToJump;
            public TextBox WhenToJump;
            public TextBox HowToJump;
            public TextBox ActivityToAdd;
            public Button AddChild;

            public ActivityUi()
            {
                Controls.Add(TextBlock = new TextBlock());
                Controls.Add(WhereToJump = new TextBox {Label = "Where to jump"});
                Controls.Add(WhenToJump = new TextBox {Label = "When to Jumo", Value = "Run"});
                Controls.Add(HowToJump = new TextBox { Label = "How to jump" });
                Controls.Add(ActivityToAdd = new TextBox { Label = "Activity to add" });
                Controls.Add(AddChild = new Button { Label = "Add child activity"});
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

        private void RunCore()
        {
            SetResponse((ActivityResponse) Enum.Parse(typeof (ActivityResponse), ConfigurationControls.HowToJump.Value), null, Guid.Parse(ConfigurationControls.WhereToJump.Value));
        }

        protected override Task RunCurrentActivity()
        {
            Log($"{CurrentActivity.Label} [{CurrentActivity.Id}] started");

            if (String.Equals(ConfigurationControls.WhenToJump.Value, "Run", StringComparison.InvariantCultureIgnoreCase))
            {
                RunCore();
            }

            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            Log($"{CurrentActivity.Label} [{CurrentActivity.Id}] ended");

            if (String.Equals(ConfigurationControls.WhenToJump.Value, "children", StringComparison.InvariantCultureIgnoreCase))
            {
                RunCore();
            }

            return Task.FromResult(0);
        }
    }
}