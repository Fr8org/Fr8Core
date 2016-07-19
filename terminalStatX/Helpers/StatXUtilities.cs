using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;
using terminalStatX.DataTransferObjects.StatTypesModels;
using terminalStatX.Infrastructure;

namespace terminalStatX.Helpers
{
    public class StatXUtilities
    {
        private const string AdvisoryName = "StatX Warning";
        private const string AdvisoryContent = "There is a stat with missing Title value. Please insert the Title value inside your StatX mobile app for this stat, so it can have understandable name.";

        public static Dictionary<string, string> StatTypesDictionary = new Dictionary<string, string>()
        {
            { "NUMBER", "Number Stat" },
            { "RANGE", "Range Stat" },
            { "CHECK_LIST", "Checklist Stat" },
            { "PICK_LIST", "Picklist Stat" },
            { "HORIZONTAL_BARS", "Horizontal Bar Stat" },
        };

        /// <summary>
        /// Based on selected Stat type get the stat properties that need to be rendered inside the ActivityUI so the user can put values for them
        /// </summary>
        /// <param name="statType"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetStatTypeProperties(string statType)
        {
            var statTypeProperties = new List<PropertyInfo>();
            BaseStatDTO stat;
            switch (statType)
            {
                case StatTypes.Number:
                    stat = new NumberStatDTO();
                    break;
                case StatTypes.Range:
                    stat = new RangeStatDTO();
                    break;
                case StatTypes.CheckList:
                    stat = new CheckListStatDTO();
                    break;
                case StatTypes.HorizontalBars:
                    stat = new HorizontalBarsStatDTO();
                    break;
                case StatTypes.PickList:
                    stat = new PicklistStatDTO();
                    break;
                default:
                    return statTypeProperties;
            }

            return stat.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(RenderUiPropertyAttribute), false)).ToList();
        }

        /// <summary>
        /// For a selected stat type create a child instance of BaseStatDTO and populate all properties with values based on what user has chose in the activity
        /// </summary>
        /// <param name="statType"></param>
        /// <param name="statProperties">Key value pairs for all available stat properties based on type with name of property and value for it</param>
        /// <param name="statItems">Key value pair for stat types that has items collection proeperty</param>
        /// <returns></returns>
        public static BaseStatDTO CreateStatFromDynamicStatProperties(string statType, List<KeyValueDTO> statProperties, List<KeyValueDTO> statItems)
        {
            BaseStatDTO stat;
            switch (statType)
            {
                case StatTypes.Number:
                    stat = new NumberStatDTO();
                    PopulateStatObject(stat, statProperties);
                    break;
                case StatTypes.Range:
                    stat = new RangeStatDTO();
                    PopulateStatObject(stat, statProperties);
                    break;
                case StatTypes.CheckList:
                    stat = new CheckListStatDTO();
                    PopulateStatObject(stat, statProperties);
                    foreach (var statItem in statItems)
                    {
                        ((CheckListStatDTO)stat).Items.Add(new CheckListItemDTO() { Name = statItem.Key, Checked = ConvertChecklistItemValue(statItem.Value)});
                    }
                    break;
                case StatTypes.HorizontalBars:
                    stat = new HorizontalBarsStatDTO { DynamicJsonIgnoreProperties = new[] { "checked" } };
                    PopulateStatObject(stat, statProperties);
                    ((HorizontalBarsStatDTO)stat).Items = statItems.Select(x => new StatItemValueDTO() { Name = x.Key, Value = x.Value}).ToList();
                    break;
                case StatTypes.PickList:
                    stat = new PicklistStatDTO {DynamicJsonIgnoreProperties = new[] {"value"}};
                    PopulateStatObject(stat, statProperties);
                    ((PicklistStatDTO)stat).Items = statItems.Select(x => new PicklistItemDTO() { Name = x.Key, Color = string.IsNullOrEmpty(x.Value) ? "UNKNOWN" : x.Value.ToUpper()}).ToList();
                    break;
                default:
                    return new BaseStatDTO();
            }

            return stat;
        }

        /// <summary>
        /// Populate values into stat properties for all RenderedUi properties in the Activity 
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="statProperties"></param>
        private static void PopulateStatObject(BaseStatDTO stat, List<KeyValueDTO> statProperties)
        {
            stat.LastUpdatedDateTime = DateTime.UtcNow;
            stat.NotesLastUpdatedDateTime = DateTime.UtcNow;

            var statPropertyInfo = stat.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(RenderUiPropertyAttribute), false)).ToList();
            
            foreach (var property in statPropertyInfo)
            {
                var payloadItem = statProperties.FirstOrDefault(x => x.Key == property.Name);
                if (payloadItem != null)
                {
                    property.SetValue(stat, Convert.ChangeType(payloadItem.Value, property.PropertyType), null);
                }
            }
        }

        /// <summary>
        /// Create Simple Crate Manifest used for polling process
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public static StatXItemCM MapToStatItemCrateManifest(BaseStatDTO stat)
        {
            var result = new StatXItemCM()
            {
                Id = stat.Id,
                Title = stat.Title,
                VisualType = stat.VisualType,
                LastUpdatedDateTime = stat.LastUpdatedDateTime.ToString(),
            };

            var statDTO = stat as GeneralStatDTO;
            if (statDTO != null)
            {
                result.Value = statDTO.Value;
                result.CurrentIndex = statDTO.CurrentIndex;
            }
            else
            {
                result.CurrentIndex = ((GeneralStatWithItemsDTO) stat).CurrentIndex;
                result.StatValueItems = ((GeneralStatWithItemsDTO)stat).Items.Select(x => new StatValueItemDTO()
                {
                    Name = x.Name,
                    Value = x.Value,
                    Checked = x.Checked,
                }).ToList();
            }

            return result;
        }

        /// <summary>
        /// Helper method used into StatX polling, Compare latest poll item with previous for value changes
        /// </summary>
        /// <param name="oldStat"></param>
        /// <param name="newStat"></param>
        /// <returns></returns>
        public static bool CompareStatsForValueChanges(StatXItemCM oldStat, StatXItemCM newStat)
        {
            if (DateTime.Parse(oldStat.LastUpdatedDateTime) >= DateTime.Parse(newStat.LastUpdatedDateTime))
                return false;

            if (newStat.StatValueItems.Any())
            {
                foreach (var item in newStat.StatValueItems)
                {
                    var oldStatItem = oldStat.StatValueItems.FirstOrDefault(x => x.Name == item.Name);

                    //case for checklist
                    if (oldStatItem == null) continue;
                    if (newStat.VisualType == StatTypes.CheckList)
                    {
                        if (item.Checked != oldStatItem.Checked)
                        {
                            return true;
                        }
                    }
                    else if (newStat.VisualType == StatTypes.PickList)
                    {
                        if (newStat.CurrentIndex != oldStat.CurrentIndex)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (item.Value != oldStatItem.Value)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return oldStat.Value != newStat.Value;
        }

        /// <summary>
        /// Add Advisory messages in case some user stat doesn't have inserted stat titles. Tell the users to update their mobile app
        /// </summary>
        /// <param name="storage"></param>
        public static void AddAdvisoryMessage(ICrateStorage storage)
        {
            var advisoryCrate = storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault();
            var currentAdvisoryResults = advisoryCrate == null ? new AdvisoryMessagesCM() : advisoryCrate.Content;

            var advisory = currentAdvisoryResults.Advisories.FirstOrDefault(x => x.Name == AdvisoryName);

            if (advisory == null)
            {
                currentAdvisoryResults.Advisories.Add(new AdvisoryMessageDTO { Name = AdvisoryName, Content = AdvisoryContent });
            }
            else
            {
                advisory.Content = AdvisoryContent;
            }

            storage.Add(Crate.FromContent("Advisories", currentAdvisoryResults));
        }

        public static bool ConvertChecklistItemValue(string value)
        {
            return value.Trim() == "1" || value.Trim().ToLower() == "true";
        }

        public static StatXAuthDTO GetStatXAuthToken(string token)
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(token);
        }

        public static StatXAuthDTO GetStatXAuthToken(AuthorizationToken token)
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(token.Token);
        }
    }
}