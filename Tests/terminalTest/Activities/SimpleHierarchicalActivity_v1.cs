using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Models;

namespace terminalTest.Actions
{
    public class SimpleHierarchicalActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SimpleHierarchicalActivity",
            Label = "SimpleHierarchicalActivity",
            Version = "1",
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
        }

        public SimpleHierarchicalActivity_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override async Task Initialize()
        {
            var templates = await HubCommunicator.GetActivityTemplates();
            var activityTemplate = templates.Select(x => new ActivityTemplateSummaryDTO
            {
                Name = x.Name,
                Version = x.Version,
                TerminalName = x.Terminal.Name,
                TerminalVersion = x.Terminal.Version
            }).First(x => x.Name == "SimpleActivity");

            ActivityPayload.ChildrenActivities.Add(new ActivityPayload
            {
                Id = Guid.NewGuid(),
                CrateStorage = new CrateStorage(),
                ActivityTemplate = activityTemplate,
                //ActivityTemplateId = activityTemplate.Id,
                Label = "main 1.1",
                Ordering = 1
            });

            ActivityPayload.ChildrenActivities.Add(new ActivityPayload
            {
                Id = Guid.NewGuid(),
                CrateStorage = new CrateStorage(),
                //ActivityTemplateId = activityTemplate.Id,
                ActivityTemplate = activityTemplate,
                Label = "main 1.2",
                Ordering = 2,
                ChildrenActivities = 
                {
                    new ActivityPayload
                    {
                        Id = Guid.NewGuid(),
                        CrateStorage = new CrateStorage(),
                        //ActivityTemplateId = activityTemplate.Id,
                        ActivityTemplate = activityTemplate,
                        Label = "main 1.2.1",
                        Ordering = 1
                    },
                    new ActivityPayload
                    {
                        Id = Guid.NewGuid(),
                        CrateStorage = new CrateStorage(),
                        //ActivityTemplateId = activityTemplate.Id,
                        ActivityTemplate = activityTemplate,
                        Label = "main 1.2.2",
                        Ordering = 2
                    }
                }
            });

            ActivityPayload.ChildrenActivities.Add(new ActivityPayload
            {
                Id = Guid.NewGuid(),
                CrateStorage = new CrateStorage(),
                //ActivityTemplateId = activityTemplate.Id,
                ActivityTemplate = activityTemplate,
                Label = "main 1.3",
                Ordering = 3,
            });
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            Log($"{ActivityPayload.Label} started");
            return Task.FromResult(0);
        }


        public override Task RunChildActivities()
        {
            Log($"{ActivityPayload.Label} ended");

            return Task.FromResult(0);
        }
    }
}