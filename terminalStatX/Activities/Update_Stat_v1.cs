using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;
using terminalStatX.Interfaces;

namespace terminalStatX.Activities
{
    public class Update_Stat_v1 : TerminalActivity<Update_Stat_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Update_Stat",
            Label = "Update Stat",
            Version = "1",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string SelectedGroupCrateLabel = "Selected Group";

        private string SelectedGroup
        {
            get
            {
                var storedValue = Storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == SelectedGroupCrateLabel);
                return storedValue?.Content.Fields.First().Key;
            }
            set
            {
                Storage.RemoveByLabel(SelectedGroupCrateLabel);
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                Storage.Add(Crate<FieldDescriptionsCM>.FromContent(SelectedGroupCrateLabel, new FieldDescriptionsCM(new FieldDTO(value)), AvailabilityType.Configuration));
            }
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList ExistingGroupsList { get; set; }

            public DropDownList ExistingGroupStats { get; set; }

            [DynamicControls]
            public List<TextSource> StatValues { get; set; }

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
                    Label = "Choose a Stat from selected Group",
                    Name = nameof(ExistingGroupStats),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                StatValues = new List<TextSource>();

                Controls = new List<ControlDefinitionDTO>() {ExistingGroupsList, ExistingGroupStats};
            }

            public void ClearDynamicFields()
            {
                StatValues?.Clear();
            }
        }

        private readonly IStatXIntegration _statXIntegration;

        public Update_Stat_v1(ICrateManager crateManager, IStatXIntegration statXIntegration) : base(crateManager)
        {
            _statXIntegration = statXIntegration;
        }

        public override async Task Initialize()
        {
            ActivityUI.ExistingGroupsList.ListItems = (await _statXIntegration.GetGroups(GetStatXAuthToken()))
                .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
        }

        private StatXAuthDTO GetStatXAuthToken()
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(AuthorizationToken.Token);
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

                        if (firstStat.StatItems.Any())
                        {
                            foreach (var item in firstStat.StatItems)
                            {
                                ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(item.Name, item.Name, requestUpstream: true));
                            }
                        }
                        else
                        {
                            ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(firstStat.Title, firstStat.Title, requestUpstream: true));
                        }
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

            if (string.IsNullOrEmpty(ActivityUI.ExistingGroupStats.Value))
            {
                ActivityUI.ClearDynamicFields();
            }
        }

        public override async Task Run()
        {
            var statValues = ActivityUI.StatValues.Select(x => new { x.Name, Value = x.GetValue(Payload) }).ToDictionary(x => x.Name, x => x.Value);
            
            if (string.IsNullOrEmpty(ActivityUI.ExistingGroupsList.Value))
            {
                throw new ActivityExecutionException("Update Stat activity run failed!. Activity doesn't have selected Group.");
            }

            if (string.IsNullOrEmpty(ActivityUI.ExistingGroupStats.Value))
            {
                throw new ActivityExecutionException("Update Stat activity run failed!. Activity doesn't have selected Stat.");
            }

            await _statXIntegration.UpdateStatValue(GetStatXAuthToken(), ActivityUI.ExistingGroupsList.Value,
                ActivityUI.ExistingGroupStats.Value, statValues);
            
            Success();
        }
    }
}