using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public class SimpleHierarchicalActivity_v1 : EnhancedTerminalActivity<SimpleActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
        }

        public SimpleHierarchicalActivity_v1() 
            : base(false)
        {
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var templates = await HubCommunicator.GetActivityTemplates(CurrentFr8UserId);
            var activityTemplate = templates.First(x => x.Name == "SimpleActivity");

            var atdo = AutoMapper.Mapper.Map<ActivityTemplateDTO, ActivityTemplateDO>(activityTemplate);

            CurrentActivity.ChildNodes.Add(new ActivityDO
            {
                Id = Guid.NewGuid(),
                ActivityTemplate = atdo,
                ActivityTemplateId = activityTemplate.Id,
                Label = "main 1.1",
                Ordering = 1
            });

            CurrentActivity.ChildNodes.Add(new ActivityDO
            {
                Id = Guid.NewGuid(),
                ActivityTemplateId = activityTemplate.Id,
                ActivityTemplate = atdo,
                Label = "main 1.2",
                Ordering = 2,
                ChildNodes =
                {
                    new ActivityDO
                    {
                        Id = Guid.NewGuid(),
                        ActivityTemplateId = activityTemplate.Id,
                        ActivityTemplate = atdo,
                        Label = "main 1.2.1",
                        Ordering = 1
                    },
                    new ActivityDO
                    {
                        Id = Guid.NewGuid(),
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
            File.AppendAllText(@"C:\Work\fr8_research\log.txt", $"{CurrentActivity.Label} started");
            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            File.AppendAllText(@"C:\Work\fr8_research\log.txt", $"{CurrentActivity.Label} ended");

            return Task.FromResult(0);
        }
    }
}