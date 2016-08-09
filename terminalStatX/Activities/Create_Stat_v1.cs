using System;
using System.Collections;
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
using Fr8.TerminalBase.Infrastructure;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;
using terminalStatX.Helpers;
using terminalStatX.Interfaces;

namespace terminalStatX.Activities
{
    public class Create_Stat_v1 : TerminalActivity<Create_Stat_v1.ActivityUi>
    {
        private readonly IStatXIntegration _statXIntegration;

        public Create_Stat_v1(ICrateManager crateManager, IStatXIntegration statXIntegration) : base(crateManager)
        {
            _statXIntegration = statXIntegration;
        }

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("28ea95d8-7d57-4be3-a2c5-bf6f1c5f4386"),
            Name = "Create_Stat",
            Label = "Create Stat",
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

        private string SelectedStatType
        {
            get { return this[nameof(SelectedStatType)]; }
            set { this[nameof(SelectedStatType)] = value; }
        }

        private string SelectedStatGroup
        {
            get { return this[nameof(SelectedStatGroup)]; }
            set { this[nameof(SelectedStatGroup)] = value; }
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public RadioButtonOption UseNewStatXGroupOption { get; set; }

            public TextBox NewStatXGroupName { get; set; }

            public RadioButtonOption UseExistingStatXGroupOption { get; set; }

            public DropDownList ExistingStatGroupList { get; set; }

            public RadioButtonGroup StatXGroupsSelectionGroup { get; set; }

            public DropDownList StatTypesList { get; set; }

            [DynamicControls]
            public List<TextSource> AvailableStatProperties { get; set; }

            [DynamicControls]
            public List<FieldList> AvailableStatItemsList { get; set; }

            public ActivityUi()
            {
                NewStatXGroupName = new TextBox
                {
                    Value = "New StatX Group",
                    Name = nameof(NewStatXGroupName)
                };

                ExistingStatGroupList = new DropDownList
                {
                    Name = nameof(ExistingStatGroupList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                UseNewStatXGroupOption = new RadioButtonOption
                {
                    Selected = false,
                    Name = nameof(UseNewStatXGroupOption),
                    Value = "in a new Group",
                    Controls = new List<ControlDefinitionDTO> { NewStatXGroupName }
                };

                UseExistingStatXGroupOption = new RadioButtonOption()
                {
                    Selected = true,
                    Name = nameof(UseExistingStatXGroupOption),
                    Value = "in an existing Group",
                    Controls = new List<ControlDefinitionDTO> { ExistingStatGroupList }
                };

                StatXGroupsSelectionGroup = new RadioButtonGroup
                {
                    Label = "Create Stat",
                    GroupName = nameof(StatXGroupsSelectionGroup),
                    Name = nameof(StatXGroupsSelectionGroup),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Radios = new List<RadioButtonOption>
                            {
                                UseNewStatXGroupOption,
                                UseExistingStatXGroupOption,
                            }
                };

                StatTypesList = new DropDownList
                {
                    Label = "Choose a Stat Type",
                    Name = nameof(StatTypesList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Controls = new List<ControlDefinitionDTO>() { StatXGroupsSelectionGroup, StatTypesList};
                AvailableStatProperties = new List<TextSource>();
                AvailableStatItemsList = new List<FieldList>();
            }

            public void ClearDynamicFields()
            {
                AvailableStatProperties?.Clear();
                AvailableStatItemsList?.Clear();
            }
        }
        public override async Task Initialize()
        {
            ActivityUI.StatTypesList.ListItems = StatXUtilities.StatTypesDictionary.Select(x => new ListItem() {Key = x.Value, Value = x.Key}).ToList();
            ActivityUI.ExistingStatGroupList.ListItems =  (await _statXIntegration.GetGroups(StatXUtilities.GetStatXAuthToken(AuthorizationToken))).Select(x => new ListItem {Key = x.Name, Value = x.Id}).ToList();
        }

        public override async Task FollowUp()
        {
            if (!string.IsNullOrEmpty(ActivityUI.StatTypesList.Value))
            {
                var previousGroup = SelectedStatType;
                if (string.IsNullOrEmpty(previousGroup) || !string.Equals(previousGroup, ActivityUI.StatTypesList.Value))
                {
                    var propertyInfos = StatXUtilities.GetStatTypeProperties(ActivityUI.StatTypesList.Value);
                    ActivityUI.ClearDynamicFields();
                    foreach (var property in propertyInfos)
                    {
                        var t = property.PropertyType;
                        if (t.IsGenericType && typeof(IList<>).IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                            t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)))
                        {
                            //render corresponding FieldList control for properties that are collections
                            ActivityUI.AvailableStatItemsList.Add(CreateFieldListBasedOnStatType());
                        }
                        else
                        {
                            ActivityUI.AvailableStatProperties.Add(UiBuilder.CreateSpecificOrUpstreamValueChooser(property.Name, property.Name, requestUpstream: true, groupLabelText: "Available Stat Properties"));
                        }
                    }
                }
                SelectedStatType = ActivityUI.StatTypesList.Value;
            }
            else
            {
                ActivityUI.ClearDynamicFields();
                SelectedStatType = string.Empty;
            }

            SelectedStatGroup = ActivityUI.ExistingStatGroupList.Value;
            ActivityUI.ExistingStatGroupList.ListItems = (await _statXIntegration.GetGroups(StatXUtilities.GetStatXAuthToken(AuthorizationToken)))
                .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
            ActivityUI.ExistingStatGroupList.Value = SelectedStatGroup;
        }

        public async override Task Run()
        {
            string groupId = ActivityUI.ExistingStatGroupList.Value;
            if (ActivityUI.UseNewStatXGroupOption.Selected)
            {
                //create at first new StatX group
                var statGroup = await _statXIntegration.CreateGroup(StatXUtilities.GetStatXAuthToken(AuthorizationToken), ActivityUI.NewStatXGroupName.Value);
                groupId = statGroup.Id;
            }

            var statProperties = new List<KeyValueDTO>();
            if (ActivityUI.AvailableStatProperties != null && ActivityUI.AvailableStatProperties.Any())
            {
                statProperties.AddRange(ActivityUI.AvailableStatProperties.Select(x => new KeyValueDTO() { Key = x.Name, Value = x.TextValue }).ToList());
            }

            var statItemsList = new List<KeyValueDTO>();
            if (ActivityUI.AvailableStatItemsList != null && ActivityUI.AvailableStatItemsList.Any())
            {
                statItemsList.AddRange(JsonConvert.DeserializeObject<List<KeyValueDTO>>(ActivityUI.AvailableStatItemsList.FirstOrDefault().Value));
            }

            var statDTO = StatXUtilities.CreateStatFromDynamicStatProperties(ActivityUI.StatTypesList.Value, statProperties, statItemsList);
            await _statXIntegration.CreateStat(StatXUtilities.GetStatXAuthToken(AuthorizationToken), groupId, statDTO);
            
            Success();
        }

        private FieldList CreateFieldListBasedOnStatType()
        {
            switch (ActivityUI.StatTypesList.Value)
            {
                case StatTypes.CheckList:
                    return new FieldList
                    {
                        Label = "Add Checkboxes",
                        Name = "Selected_Fields",
                        Required = true,
                        FieldLabel = "Label",
                        ValueLabel = "Value",
                        Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
                    };
                case StatTypes.HorizontalBars:
                    return new FieldList
                    {
                        Label = "Add Horizontal Bar Items",
                        Name = "Selected_Fields",
                        FieldLabel = "Label",
                        ValueLabel = "Value",
                        Required = true,
                        Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
                    };
                case StatTypes.PickList:
                    return new FieldList
                    {
                        Label = "Add Picklist Items",
                        Name = "Selected_Fields",
                        FieldLabel = "Label",
                        ValueLabel = "Color",
                        Required = true,
                        Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
                    };
                default:
                    return new FieldList
                    {
                        Label = "Add Multiple Stat Items",
                        Name = "Selected_Fields",
                        Required = true,
                        Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
                    };
            }
        }
    }
}