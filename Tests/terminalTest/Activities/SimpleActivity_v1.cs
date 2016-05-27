using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalTest.Actions
{
    public class SimpleActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SimpleActivity",
            Label = "SimpleActivity",
            Category = ActivityCategory.Processors,
            Version = "1",
            WebService = TerminalData.WebServiceDTO,
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

        protected override Task InitializeETA()
        {
            ActivityUI.TextBlock.Value = ActivityId.ToString();
            return Task.FromResult(0);
        }

        protected override async Task ConfigureETA()
        {
            if (ActivityUI.AddChild.Clicked)
            {
                ActivityUI.AddChild.Clicked = false;
                var activityTemplate = await GetActivityTemplateByName(ActivityUI.ActivityToAdd.Value);
                await AddAndConfigureChildActivity(ActivityId, activityTemplate, ActivityPayload.Label + "." + (ActivityPayload.ChildrenActivities.Count + 1), ActivityPayload.Label + "." + (ActivityPayload.ChildrenActivities.Count + 1));
            }
        }

        

        protected override Task RunETA()
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