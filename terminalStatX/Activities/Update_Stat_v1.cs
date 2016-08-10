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
using terminalStatX.Helpers;
using terminalStatX.Interfaces;
using System;

namespace terminalStatX.Activities
{
    public class Update_Stat_v1 : TerminalActivity<Update_Stat_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("c8a29957-972d-447d-ad08-e8c66f5b62dc"),
            Name = "Update_Stat",
            Label = "Update Stat",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private string SelectedGroup
        {
            get { return this["SelectedGroup"]; }
            set { this["SelectedGroup"] = value; }
        }

        private string SelectedStat
        {
            get { return this["SelectedStat"]; }
            set { this["SelectedStat"] = value; }
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList ExistingGroupsList { get; set; }

            public DropDownList ExistingGroupStats { get; set; }

            public TextSource StatTitle { get; set; }

            public TextSource StatNotes { get; set; }
                
            [DynamicControls]
            public List<TextSource> StatValues { get; set; }

            public ActivityUi()
            {
                ExistingGroupsList = new DropDownList
                {
                    Label = "Choose a StatX Group",
                    Name = nameof(ExistingGroupsList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                ExistingGroupStats = new DropDownList
                {
                    Label = "Update which Stat?",
                    Name = nameof(ExistingGroupStats),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                StatTitle = new TextSource("Title", CrateManifestTypes.StandardDesignTimeFields, nameof(StatTitle), "Available Stat Properties");

                StatNotes = new TextSource("Notes", CrateManifestTypes.StandardDesignTimeFields, nameof(StatNotes), "Available Stat Properties");

                StatValues = new List<TextSource>();

                Controls = new List<ControlDefinitionDTO>() {ExistingGroupsList, ExistingGroupStats, StatTitle, StatNotes};
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
                        ActivityUI.StatTitle.TextValue = firstStat.Title;
                        ActivityUI.StatNotes.TextValue = firstStat.Notes;
                        var statDTO = firstStat as GeneralStatWithItemsDTO;
                        if (statDTO != null)
                        {
                            if (statDTO.VisualType == StatTypes.PickList)
                            {
                                ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser("Current Index", "CurrentIndex", requestUpstream: true, groupLabelText: "Available Stat Properties"));
                            }
                            else
                            {
                                foreach (var item in statDTO.Items)
                                {
                                    ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(item.Name, item.Name, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                                }
                            }
                        }
                        else
                        {
                            ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(string.IsNullOrEmpty(firstStat.Title) ? firstStat.Id : firstStat.Title, string.IsNullOrEmpty(firstStat.Title) ? firstStat.Id : firstStat.Title, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                        }
                    }
                }
                SelectedGroup = ActivityUI.ExistingGroupsList.Value;
                //refresh statx groups
                ActivityUI.ExistingGroupsList.ListItems = (await _statXIntegration.GetGroups(StatXUtilities.GetStatXAuthToken(AuthorizationToken)))
                     .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
                ActivityUI.ExistingGroupsList.Value = SelectedGroup;

            }
            else
            {
                ActivityUI.StatTitle.Value = string.Empty;
                ActivityUI.StatNotes.Value = string.Empty;
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
                        ActivityUI.ClearDynamicFields();
                        var statDTO = currentStat as GeneralStatWithItemsDTO;
                        if (statDTO != null && statDTO.Items.Any())
                        {
                            if (statDTO.VisualType == StatTypes.PickList)
                            {
                                ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser("Current Index", "CurrentIndex", requestUpstream: true, groupLabelText: "Available Stat Properties"));
                            }
                            else
                            {
                                foreach (var item in statDTO.Items)
                                {
                                    ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(item.Name, item.Name, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                                }
                            }
                        }
                        else
                        {
                            ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(string.IsNullOrEmpty(currentStat.Title) ? currentStat.Id : currentStat.Title, string.IsNullOrEmpty(currentStat.Title) ? currentStat.Id : currentStat.Title, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                        }
                    }
                }

                #region Refresh Stat Items to Track for Changes in app
                SelectedStat = ActivityUI.ExistingGroupStats.Value;
                ActivityUI.ExistingGroupStats.ListItems = stats.Select(x => new ListItem { Key = string.IsNullOrEmpty(x.Title) ? x.Id : x.Title, Value = x.Id }).ToList();
                ActivityUI.ExistingGroupStats.Value = SelectedStat;
                var statToCheck = stats.FirstOrDefault(x => x.Id == SelectedStat);


                //check for changes in statValue Array
                if (statToCheck != null)
                {
                    ActivityUI.ExistingGroupStats.selectedKey = string.IsNullOrEmpty(statToCheck.Title) ? statToCheck.Id : statToCheck.Title;
                    var statDTO = statToCheck as GeneralStatWithItemsDTO;
                    if (statDTO != null && statDTO.Items.Any())
                    {
                        var oldStatNames = ActivityUI.StatValues.Select(x => x.Name).ToList();
                        var newStatNames = new List<string>();

                        if (statDTO.VisualType != StatTypes.PickList)
                        {
                            newStatNames = statDTO.Items.Select(x => x.Name).ToList();

                            //recreate new items 
                            var newItems = newStatNames.Where(x => !oldStatNames.Contains(x)).ToList();
                            var oldItemsToDelete = oldStatNames.Where(x => !newStatNames.Contains(x)).ToList();

                            foreach (var item in oldItemsToDelete.ToList())
                            {
                                var itemToDelete = ActivityUI.StatValues.FirstOrDefault(x => x.Name == item);
                                if (itemToDelete != null)
                                {
                                    ActivityUI.StatValues.Remove(itemToDelete);
                                }
                            }

                            foreach (var item in newItems)
                            {
                                ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(item, item, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                            }
                        }
                    }
                    else
                    {
                        var currentTextSource = ActivityUI.StatValues.FirstOrDefault()?.Name;
                        if (currentTextSource != (string.IsNullOrEmpty(statToCheck.Title) ? statToCheck.Id : statToCheck.Title))
                        {
                            ActivityUI.StatValues?.Clear();
                            ActivityUI.StatValues.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(string.IsNullOrEmpty(statToCheck.Title) ? statToCheck.Id : statToCheck.Title, string.IsNullOrEmpty(statToCheck.Title) ? statToCheck.Id : statToCheck.Title, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                        }
                    }
                }

                #endregion
            }
            else
            {
                ActivityUI.StatTitle.Value = string.Empty;
                ActivityUI.StatNotes.Value = string.Empty;
                ActivityUI.ClearDynamicFields();
                ActivityUI.ExistingGroupStats.ListItems.Clear();
                ActivityUI.ExistingGroupStats.selectedKey = string.Empty;
                ActivityUI.ExistingGroupStats.Value = string.Empty;
                SelectedStat = string.Empty;
            }
        }

        public override async Task Run()
        {
            var statValues = ActivityUI.StatValues.Select(x => new { x.Name, Value = x.TextValue }).ToDictionary(x => x.Name, x => x.Value);
            
            if (string.IsNullOrEmpty(ActivityUI.ExistingGroupsList.Value))
            {
                throw new ActivityExecutionException("Update Stat activity run failed!. Activity doesn't have selected Group.");
            }   

            if (string.IsNullOrEmpty(ActivityUI.ExistingGroupStats.Value))
            {
                throw new ActivityExecutionException("Update Stat activity run failed!. Activity doesn't have selected Stat.");
            }

            await _statXIntegration.UpdateStatValue(StatXUtilities.GetStatXAuthToken(AuthorizationToken), ActivityUI.ExistingGroupsList.Value,
                ActivityUI.ExistingGroupStats.Value, statValues, ActivityUI.StatTitle.TextValue, ActivityUI.StatNotes.TextValue);
            
            Success();
        }
    }
}