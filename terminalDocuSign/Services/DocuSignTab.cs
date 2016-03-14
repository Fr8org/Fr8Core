using Data.Control;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Services.NewApi
{
    public static class DocuSignTab
    {
        public static IEnumerable<DocuSignTabDTO> ExtractTabs(JObject tabs, string roleName)
        {

            var envelopeData = new List<DocuSignTabDTO>();
            envelopeData.AddRange(AddEnvelopeData(tabs, "textTabs", "value", roleName));
            envelopeData.AddRange(AddEnvelopeData(tabs, "companyTabs", "value", roleName));
            envelopeData.AddRange(AddEnvelopeData(tabs, "titleTabs", "value", roleName));
            envelopeData.AddRange(AddEnvelopeData(tabs, "noteTabs", "value", roleName));
            envelopeData.AddRange(AddEnvelopeData(tabs, "numberTabs", "value", roleName));
            envelopeData.AddRange(AddEnvelopeData(tabs, "formulaTabs", "value", roleName));
            if (tabs["checkboxTabs"] != null)
            {
                envelopeData.AddRange(AddEnvelopeData(tabs, "checkboxTabs", "selected", roleName));
            }

            //extract Date data and add it to envelope 
            if (tabs["dateTabs"] != null)
            {
                //TODO: implement date source control first and revisit this part 
            }

            //create radio button groups
            if (tabs["radioGroupTabs"] != null)
            {
                envelopeData.AddRange(AddRadioButtonGroupEnvelopeData(tabs, "radioGroupTabs", "radios", roleName));
            }

            //create dropdown control as envelope data from tabs
            if (tabs["listTabs"] != null)
            {
                envelopeData.AddRange(AddDropdownListEnvelopeData(tabs, "listTabs", "listItems", roleName));
            }


            return envelopeData;
        }

        public static IEnumerable<FieldDTO> GetEnvelopeTabsPerSigner(JObject tabs, string roleName)
        {
            return MapTabsToFieldDTO(ExtractTabs(tabs, roleName));
        }

        public static IEnumerable<FieldDTO> MapTabsToFieldDTO(IEnumerable<DocuSignTabDTO> tabs)
        {
            var result = new List<FieldDTO>();

            foreach (var tab in tabs)
            {
                if (tab is DocuSignTabDTO)
                {
                    result.Add(new FieldDTO(tab.Name, tab.Value) { Tags = "DocuSignTab, recipientId:" + tab.RecipientId });
                }
                else
                    if (tab is DocuSignMultipleOptionsTabDTO)
                {
                    var value = (tab as DocuSignMultipleOptionsTabDTO).Items.Where(a => a.Selected).FirstOrDefault();
                    result.Add(
                        new FieldDTO()
                        {
                            Key = tab.Name,
                            Value = value?.Value,
                            Tags = "DocuSignTab, recipientId:" + tab.RecipientId
                        });
                }
            }
            return result;
        }


        #region private methods

        private static DocuSignTabDTO CreateEnvelopeData(dynamic tab, string value, string roleName, string tabName)
        {
            return new DocuSignTabDTO
            {
                DocumentId = tab["documentId"],
                RecipientId = tab["recipientId"],
                Name = string.Format("{0}({1})", tab["tabLabel"], roleName),
                TabId = tab["tabId"],
                Value = value,
                Type = GetFieldType(tabName),
                RoleName = roleName,
                TabName = tabName
            };
        }

        private static string GetFieldType(string name)
        {
            switch (name)
            {
                case "checkboxTabs":
                    return ControlTypes.CheckBox;
                case "dateTabs":
                    return ControlTypes.DatePicker;
                default:
                    return ControlTypes.TextBox;
            }
        }

        private static IEnumerable<DocuSignTabDTO> AddEnvelopeData(JToken tabs, string tabName, string tabField, string roleName)
        {
            if (tabs[tabName] != null)
            {
                foreach (var textTab in tabs[tabName])
                {
                    yield return CreateEnvelopeData(textTab, textTab[tabField].ToString(), roleName, tabName);
                }
            }
        }

        private static IEnumerable<DocuSignTabDTO> AddRadioButtonGroupEnvelopeData(JToken tabs, string tabName, string radiosName, string roleName)
        {
            if (tabs[tabName] != null)
            {
                foreach (var groupTab in tabs[tabName])
                {
                    dynamic grpTab = groupTab;
                    var groupWrapperEnvelopeData = new DocuSignMultipleOptionsTabDTO()
                    {
                        DocumentId = grpTab["documentId"],
                        RecipientId = grpTab["recipientId"],
                        Name = string.Format("{0}({1})", grpTab["groupName"], roleName),
                        TabId = grpTab["tabId"],
                        Type = ControlTypes.RadioButtonGroup,
                        RoleName = roleName,
                        TabName = tabName
                    };

                    if (groupTab[radiosName] != null)
                    {
                        foreach (var listItem in groupTab[radiosName])
                        {
                            dynamic lstItem = listItem;
                            groupWrapperEnvelopeData.Items.Add(new DocuSignOptionItemTabDTO()
                            {
                                Value = lstItem["value"],
                                Selected = lstItem["selected"]
                            });
                        }
                    }

                    yield return groupWrapperEnvelopeData;
                }
            }
        }

        private static IEnumerable<DocuSignTabDTO> AddDropdownListEnvelopeData(JToken tabs, string tabName, string listItemsName, string roleName)
        {
            if (tabs[tabName] != null)
            {
                foreach (var groupTab in tabs[tabName])
                {
                    dynamic grpTab = groupTab;
                    var groupWrapperEnvelopeData = new DocuSignMultipleOptionsTabDTO
                    {
                        DocumentId = grpTab["documentId"],
                        RecipientId = grpTab["recipientId"],
                        Name = string.Format("{0}({1})", grpTab["tabLabel"], roleName),
                        TabId = grpTab["tabId"],
                        Type = ControlTypes.DropDownList,
                        Value = grpTab["value"],
                        RoleName = roleName,
                        TabName = tabName
                    };

                    if (groupTab[listItemsName] != null)
                    {
                        foreach (var listItem in groupTab[listItemsName])
                        {
                            dynamic lstItem = listItem;
                            groupWrapperEnvelopeData.Items.Add(new DocuSignOptionItemTabDTO
                            {
                                Text = lstItem["text"],
                                Value = lstItem["value"],
                                Selected = lstItem["selected"]
                            });
                        }
                    }

                    yield return groupWrapperEnvelopeData;
                }
            }
        }

        #endregion
    }
}