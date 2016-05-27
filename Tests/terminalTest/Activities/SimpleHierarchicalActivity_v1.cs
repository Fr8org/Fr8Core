using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalTest.Actions
{
    public class SimpleHierarchicalActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SimpleHierarchicalActivity",
            Label = "SimpleHierarchicalActivity",
            Category = ActivityCategory.Processors,
            Version = "1",
            WebService = TerminalData.WebServiceDTO,
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

        protected override async Task InitializeETA()
        {
            var templates = await HubCommunicator.GetActivityTemplates();
            var activityTemplate = templates.First(x => x.Name == "SimpleActivity");

            //var atdo = AutoMapper.Mapper.Map<ActivityTemplateDTO, ActivityTemplateDO>(activityTemplate);

            string emptyCrateStorage = CrateManager.CrateStorageAsStr(new CrateStorage(Crate.FromContent("Configuration Controls", new SimpleActivity_v1.ActivityUi())));

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

        protected override Task ConfigureETA()
        {
            return Task.FromResult(0);
        }

        protected override Task RunETA()
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