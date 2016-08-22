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
using terminalStatX.Helpers;
using terminalStatX.Interfaces;

namespace terminalStatX.Activities
{
    public class Monitor_Stat_Changes_v1 : TerminalActivity<Monitor_Stat_Changes_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("47696645-2b77-4dad-9a0f-dd3b53f52063"),
            Name = "Monitor_Stat_Changes",
            Label = "Monitor Stat Changes",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };

        private readonly IStatXIntegration _statXIntegration;

        private readonly IStatXPolling _statXPolling;

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private readonly string RunTimeCrateLabel = "Stat Properties from Monitor StatX Changes";

        private string SelectedGroup
        {
            get { return this[nameof(SelectedGroup)]; }
            set { this[nameof(SelectedGroup)] = value; }
        }

        private string SelectedStat
        {
            get { return this[nameof(SelectedStat)]; }
            set { this[nameof(SelectedStat)] = value; }
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
                    Label = "Monitor which Stat?",
                    Name = nameof(ExistingGroupStats),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Controls = new List<ControlDefinitionDTO>() { ExistingGroupsList, ExistingGroupStats };
            }
        }

        public Monitor_Stat_Changes_v1(ICrateManager crateManager, IStatXIntegration statXIntegration, IStatXPolling statXPolling) : base(crateManager)
        {
            _statXIntegration = statXIntegration;
            _statXPolling = statXPolling;
        }

        public override async Task Initialize()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel);

            ActivityUI.ExistingGroupsList.ListItems = (await _statXIntegration.GetGroups(StatXUtilities.GetStatXAuthToken(AuthorizationToken)))
                .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
        }

        public override async Task FollowUp()
        {
            if (!string.IsNullOrEmpty(ActivityUI.ExistingGroupsList.Value))
            {
                var previousGroup = SelectedGroup;
                if (string.IsNullOrEmpty(previousGroup) || !string.Equals(previousGroup, ActivityUI.ExistingGroupsList.Value))
                {
                    var stats = await _statXIntegration.GetStatsForGroup(StatXUtilities.GetStatXAuthToken(AuthorizationToken), ActivityUI.ExistingGroupsList.Value);

                    if (stats.Any(x => string.IsNullOrEmpty(x.Title)))
                    {
                        StatXUtilities.AddAdvisoryMessage(Storage);
                    }
                    else
                    {
                        if (Storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault() != null)
                        {
                            ActivityUI.ExistingGroupStats.ListItems = stats.Select(x => new ListItem { Key = string.IsNullOrEmpty(x.Title) ? x.Id : x.Title, Value = x.Id }).ToList();
                        }
                        Storage.RemoveByLabel("Advisories");
                    }

                    ActivityUI.ExistingGroupStats.ListItems = stats
                        .Select(x => new ListItem { Key = string.IsNullOrEmpty(x.Title) ? x.Id : x.Title, Value = x.Id }).ToList();

                    var firstStat = stats.FirstOrDefault();
                    if (firstStat != null)
                    {
                        ActivityUI.ExistingGroupStats.SelectByValue(firstStat.Id);
                    }
                    
                    CrateSignaller.ClearAvailableCrates();
                    CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel)
                        .AddFields(CreateStatValueFields(StatXUtilities.MapToStatItemCrateManifest(firstStat)));
                }
                SelectedGroup = ActivityUI.ExistingGroupsList.Value;
                ActivityUI.ExistingGroupsList.ListItems = (await _statXIntegration.GetGroups(StatXUtilities.GetStatXAuthToken(AuthorizationToken)))
                    .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
                ActivityUI.ExistingGroupsList.Value = SelectedGroup;
            }
            else
            {
                ActivityUI.ExistingGroupStats.ListItems.Clear();
                ActivityUI.ExistingGroupStats.selectedKey = string.Empty;
                ActivityUI.ExistingGroupStats.Value = string.Empty;
                SelectedGroup = string.Empty;
            }

            if (!string.IsNullOrEmpty(ActivityUI.ExistingGroupStats.Value))
            {
                var previousStat = SelectedStat;
                var stats = await _statXIntegration.GetStatsForGroup(StatXUtilities.GetStatXAuthToken(AuthorizationToken), ActivityUI.ExistingGroupsList.Value);
                if (stats.Any(x => string.IsNullOrEmpty(x.Title)))
                {
                    StatXUtilities.AddAdvisoryMessage(Storage);
                }
                else
                {
                    if (Storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault() != null)
                    {
                        ActivityUI.ExistingGroupStats.ListItems = stats.Select(x => new ListItem { Key = string.IsNullOrEmpty(x.Title) ? x.Id : x.Title, Value = x.Id }).ToList();
                    }
                    Storage.RemoveByLabel("Advisories");
                }

                if (string.IsNullOrEmpty(previousStat) || !string.Equals(previousStat, ActivityUI.ExistingGroupStats.Value))
                {
                    var currentStat = stats.FirstOrDefault(x => x.Id == ActivityUI.ExistingGroupStats.Value);
                    if (currentStat != null)
                    {
                        CrateSignaller.ClearAvailableCrates();
                        CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel).AddFields(CreateStatValueFields(StatXUtilities.MapToStatItemCrateManifest(currentStat)));
                    }
                }
                SelectedStat= ActivityUI.ExistingGroupStats.Value;
                ActivityUI.ExistingGroupStats.ListItems = stats.Select(x => new ListItem { Key = string.IsNullOrEmpty(x.Title) ? x.Id : x.Title, Value = x.Id }).ToList();
                ActivityUI.ExistingGroupStats.Value = SelectedStat;

                EventSubscriptions.Subscriptions?.Clear();
                EventSubscriptions.Manufacturer = "StatX";
                EventSubscriptions.Add("StatXValueChange_" + SelectedStat.Substring(0, 18));
            }
            else
            {
                CrateSignaller.ClearAvailableCrates();
                ActivityUI.ExistingGroupStats.ListItems.Clear();
                ActivityUI.ExistingGroupStats.selectedKey = string.Empty;
                ActivityUI.ExistingGroupStats.Value = string.Empty;
                SelectedStat = string.Empty;
            }
        }

        public override async Task Activate()
        {
            await _statXPolling.SchedulePolling(HubCommunicator, AuthorizationToken.ExternalAccountId, true, 
                ActivityUI.ExistingGroupsList.Value, ActivityUI.ExistingGroupStats.Value);
        }

        public override Task Run()
        {
            StatXItemCM stat = null;
            var eventCrate = Payload.CratesOfType<EventReportCM>().FirstOrDefault()?.Get<EventReportCM>()?.EventPayload;
            if (eventCrate != null)
                stat = eventCrate.CrateContentsOfType<StatXItemCM>().SingleOrDefault();

            if (stat == null)
            {
                RequestPlanExecutionTermination("Stat was not found in the payload.");
            }

            Payload.Add(Crate.FromContent<StandardPayloadDataCM>(RunTimeCrateLabel, new StandardPayloadDataCM(CreateStatKeyValueItems(stat))));

            return Task.FromResult(0);
        }

        private List<FieldDTO> CreateStatValueFields(StatXItemCM stat)
        {
            var fields = new List<FieldDTO>();

            if (stat.StatValueItems.Any())
            {
                if (stat.VisualType == StatTypes.PickList)
                {
                    fields.Add(new FieldDTO("Current Index", AvailabilityType.RunTime));
                }
                else
                {
                    fields.AddRange(stat.StatValueItems.Select(item => new FieldDTO(item.Name, AvailabilityType.RunTime)));
                }
            }
            else
            {
                fields.Add(new FieldDTO(string.IsNullOrEmpty(stat.Title) ? stat.Id : stat.Title, AvailabilityType.RunTime));
            }

            return fields;
        }

        private List<KeyValueDTO> CreateStatKeyValueItems(StatXItemCM stat)
        {
            var fields = new List<KeyValueDTO>();

            if (stat.StatValueItems.Any())
            {
                if (stat.VisualType == StatTypes.PickList)
                {
                    fields.Add(new KeyValueDTO("Current Index", stat.CurrentIndex.ToString()));
                }
                else
                {
                    fields.AddRange(stat.VisualType == StatTypes.CheckList
                        ? stat.StatValueItems.Select(item => new KeyValueDTO(item.Name, item.Checked.ToString()))
                        : stat.StatValueItems.Select(item => new KeyValueDTO(item.Name, item.Value)));
                }
            }
            else
            {
                fields.Add(new KeyValueDTO(string.IsNullOrEmpty(stat.Title) ? stat.Id : stat.Title, stat.Value));
            }

            return fields;
        }
    }
}