using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalTest.Actions
{
    public class SimpleHierarchicalActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
        }
        
        protected override async Task InitializeETA()
        {
            var templates = await HubCommunicator.GetActivityTemplates(CurrentUserId);
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

        protected override ActivityTemplateDTO MyTemplate => new ActivityTemplateDTO();

        protected override Task RunChildActivities()
        {
            Log($"{ActivityPayload.Label} ended");

            return Task.FromResult(0);
        }
    }
}