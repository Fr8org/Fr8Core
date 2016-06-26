using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;
using terminalStatX.Interfaces;

namespace terminalStatX.Activities
{
    public class Monitor_Stat_Changes_v1 : TerminalActivity<Monitor_Stat_Changes_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_Stat_Changes",
            Label = "Monitor Stat Changes",
            Version = "1",
            Category = ActivityCategory.Monitors,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private readonly IStatXIntegration _statXIntegration;

        private string SelectedGroup
        {
            get { return this["SelectedGroup"]; }
            set { this["SelectedGroup"] = value; }
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList ExistingGroupsList { get; set; }

            public DropDownList ExistingGroupStats { get; set; }

            public ActivityUi()
            {
                ExistingGroupsList = new DropDownList()
                {
                    Label = "Choose a StatX Group",
                    Name = nameof(ExistingGroupsList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                ExistingGroupStats = new DropDownList()
                {
                    Label = "Choose a Stat from Selected Group",
                    Name = nameof(ExistingGroupStats),
                };

                Controls = new List<ControlDefinitionDTO>() { ExistingGroupsList, ExistingGroupStats };
            }
        }

        public Monitor_Stat_Changes_v1(ICrateManager crateManager, IStatXIntegration statXIntegration) : base(crateManager)
        {
            _statXIntegration = statXIntegration;
        }

        public override async Task Initialize()
        {
            ActivityUI.ExistingGroupsList.ListItems = (await _statXIntegration.GetGroups(GetStatXAuthToken()))
                .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
        }

        public override async Task FollowUp()
        {
            if (!string.IsNullOrEmpty(ActivityUI.ExistingGroupsList.Value))
            {
                var previousGroup = SelectedGroup;
                if (string.IsNullOrEmpty(previousGroup) || !string.Equals(previousGroup, ActivityUI.ExistingGroupsList.Value))
                {
                    var stats = await _statXIntegration.GetStatsForGroup(GetStatXAuthToken(), ActivityUI.ExistingGroupsList.Value);

                    ActivityUI.ExistingGroupStats.ListItems = stats
                        .Select(x => new ListItem { Key = x.Title, Value = x.Id }).ToList();

                    var firstStat = stats.FirstOrDefault();
                    if (firstStat != null)
                    {
                        ActivityUI.ExistingGroupStats.SelectByValue(firstStat.Id);
                    }
                }
                SelectedGroup = ActivityUI.ExistingGroupsList.Value;
            }
            else
            {
                ActivityUI.ExistingGroupStats.ListItems.Clear();
                ActivityUI.ExistingGroupStats.selectedKey = string.Empty;
                ActivityUI.ExistingGroupStats.Value = string.Empty;
                SelectedGroup = string.Empty;
            }
        }

        public override async Task Activate()
        {
            throw new NotImplementedException();
        }

        public override Task Run()
        {
            throw new NotImplementedException();
        }

        private StatXAuthDTO GetStatXAuthToken()
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(AuthorizationToken.Token);
        }
    }
}