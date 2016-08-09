using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Services;

namespace terminalTest.Actions
{
    public class SimpleJumperActivity_v1 : TestActivityBase<SimpleJumperActivity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SimpleJumperActivity",
            Label = "SimpleJumperActivity",
            Version = "1",
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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

        public SimpleJumperActivity_v1(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            ActivityUI.TextBlock.Value = ActivityId.ToString();
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            if (ActivityUI.AddChild.Clicked)
            {
                ActivityUI.AddChild.Clicked = false;
                var activityTemplate = await GetActivityTemplateByName(ActivityUI.ActivityToAdd.Value);
                await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, activityTemplate, ActivityPayload.Label + "." + (ActivityPayload.ChildrenActivities.Count + 1), ActivityPayload.Label + "." + (ActivityPayload.ChildrenActivities.Count + 1));
            }
        }

        private void RunCore()
        {
            SetResponse((ActivityResponse) Enum.Parse(typeof (ActivityResponse), ActivityUI.HowToJump.Value), null, Guid.Parse(ActivityUI.WhereToJump.Value));
        }

        public override Task Run()
        {
            Log($"{ActivityPayload.Label} [{ActivityId}] started");

            if (String.Equals(ActivityUI.WhenToJump.Value, "Run", StringComparison.InvariantCultureIgnoreCase))
            {
                RunCore();
            }

            return Task.FromResult(0);
        }


        public override Task RunChildActivities()
        {
            Log($"{ActivityPayload.Label} [{ActivityId}] ended");

            if (String.Equals(ActivityUI.WhenToJump.Value, "children", StringComparison.InvariantCultureIgnoreCase))
            {
                RunCore();
            }

            return Task.FromResult(0);
        }
    }
}