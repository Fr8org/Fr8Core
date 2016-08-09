using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Services;

namespace terminalTest.Actions
{
    public class SimpleActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SimpleActivity",
            Label = "SimpleActivity",
            Version = "1",
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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


        public SimpleActivity_v1(ICrateManager crateManager)
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



        public override Task Run()
        {
            Log($"{ActivityPayload.Label} [{ActivityId}] started");
            return Task.FromResult(0);
        }

        public override Task RunChildActivities()
        {
            Log($"{ActivityPayload.Label} [{ActivityId}] ended");
            return Task.FromResult(0);
        }
    }
}