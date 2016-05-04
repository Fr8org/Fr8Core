using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public class SimpleHierarchicalActivity_v1 : TestActivityBase<SimpleActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
        }
        
        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var templates = await HubCommunicator.GetActivityTemplates(CurrentFr8UserId);
            var activityTemplate = templates.First(x => x.Name == "SimpleActivity");

            var atdo = AutoMapper.Mapper.Map<ActivityTemplateDTO, ActivityTemplateDO>(activityTemplate);

            string emptyCrateStorage = CrateManager.CrateStorageAsStr(new CrateStorage(Crate.FromContent("Configuration Controls", new SimpleActivity_v1.ActivityUi())));

            CurrentActivity.ChildNodes.Add(new ActivityDO
            {
                Id = Guid.NewGuid(),
                CrateStorage = emptyCrateStorage,
                ActivityTemplate = atdo,
                ActivityTemplateId = activityTemplate.Id,
                Label = "main 1.1",
                Ordering = 1
            });

            CurrentActivity.ChildNodes.Add(new ActivityDO
            {
                Id = Guid.NewGuid(),
                CrateStorage = emptyCrateStorage,
                ActivityTemplateId = activityTemplate.Id,
                ActivityTemplate = atdo,
                Label = "main 1.2",
                Ordering = 2,
                ChildNodes =
                {
                    new ActivityDO
                    {
                        Id = Guid.NewGuid(),
                        CrateStorage = emptyCrateStorage,
                        ActivityTemplateId = activityTemplate.Id,
                        ActivityTemplate = atdo,
                        Label = "main 1.2.1",
                        Ordering = 1
                    },
                    new ActivityDO
                    {
                        Id = Guid.NewGuid(),
                        CrateStorage = emptyCrateStorage,
                        ActivityTemplateId = activityTemplate.Id,
                        ActivityTemplate = atdo,
                        Label = "main 1.2.2",
                        Ordering = 2
                    }
                }
            });

            CurrentActivity.ChildNodes.Add(new ActivityDO
            {
                Id = Guid.NewGuid(),
                CrateStorage = emptyCrateStorage,
                ActivityTemplateId = activityTemplate.Id,
                ActivityTemplate = atdo,
                Label = "main 1.3",
                Ordering = 3,
            });
        }

        protected override Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            Log($"{CurrentActivity.Label} started");
            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            Log($"{CurrentActivity.Label} ended");

            return Task.FromResult(0);
        }
    }
}