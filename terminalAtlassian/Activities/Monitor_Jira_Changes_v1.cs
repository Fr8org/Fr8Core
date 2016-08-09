using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalAtlassian.Interfaces;

namespace terminalAtlassian.Actions
{
    public class Monitor_Jira_Changes_v1 : TerminalActivity<Monitor_Jira_Changes_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("d2e7f4b6-5e83-4f2f-b779-925547aa9542"),
            Version = "1",
            Name = "Monitor_Jira_Changes_v1",
            Label = "Monitor Jira Changes",
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
           
        }
        private readonly IAtlassianService _atlassianService;
        private readonly IPushNotificationService _pushNotificationService;


        public Monitor_Jira_Changes_v1(ICrateManager crateManager, IAtlassianService atlassianService, IPushNotificationService pushNotificationService)
            : base(crateManager)
        {
            _atlassianService = atlassianService;
            _pushNotificationService = pushNotificationService;
        }

        #region Configuration

        public override async Task Initialize()
        {
           
            await Task.Yield();
        }

        public override async Task FollowUp()
        {

            await Task.Yield();
        }

        #endregion Configuration


        #region Runtime

        public override async Task Run()
        {
          
        }

        #endregion Runtime
    }
}
