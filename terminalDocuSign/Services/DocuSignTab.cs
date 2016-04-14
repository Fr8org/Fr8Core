using Data.Control;
using Data.Interfaces.DataTransferObjects;
using DocuSign.eSign.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using terminalDocuSign.DataTransferObjects;
using Utilities;

namespace terminalDocuSign.Services.NewApi
{

    //IMPORTANT:
    //
    // if you bring changes to the logic, responsible for processing json int DocuSignTabDTO, 
    // make sure you don't break consistency between the way DocuSignEnvelopeCM_v2 is populated by Connect events and by polling
    //
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
                    result.Add(new FieldDTO(tab.Name, tab.Value) { Tags = string.Format("DocuSignTab:{0}, recipientId:{1}", tab.TabName, tab.RecipientId) });
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
                            Tags = string.Format("DocuSignTab:{0}, recipientId:{1}", tab.TabName, tab.RecipientId)
                        });
                }
            }
            return result;
        }

        public static JObject ApplyValuesToTabs(List<FieldDTO> fieldList, Signer corresponding_template_recipient, Tabs tabs)
        {
            JObject jobj = JObject.Parse(tabs.ToJson());
            foreach (var item in jobj.Properties())
            {
                string tab_type = item.Name;
                var fields = fieldList.Where(a => a.Tags.Contains(tab_type) && a.Tags.Contains("recipientId:" + corresponding_template_recipient.RecipientId));
                foreach (JObject tab in item.Value)
                {
                    FieldDTO corresponding_field = null;
                    switch (tab_type)
                    {
                        case "radioGroupTabs":
                            corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("groupName").Value.ToString())).FirstOrDefault();
                            if (corresponding_field == null)
                                break;
                            tab["radios"].Where(a => a["value"].ToString() == corresponding_field.Value).FirstOrDefault()["selected"] = "true";
                            foreach (var radioItem in tab["radios"].Where(a => a["value"].ToString() != corresponding_field.Value).ToList())
                            {
                                radioItem["selected"] = "false";
                            }
                            break;

                        case "listTabs":
                            corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString())).FirstOrDefault();
                            if (corresponding_field == null)
                                break;
                            tab["listItems"].Where(a => a["value"].ToString() == corresponding_field.Value.Trim()).FirstOrDefault()["selected"] = "true";
                            foreach (var listItem in tab["listItems"].Where(a => a["value"].ToString() != corresponding_field.Value.Trim()))
                            {
                                //set all other to false
                                listItem["selected"] = "false";
                            }
                            //["selected"] = "true";
                            tab["value"] = corresponding_field.Value;
                            break;
                        case "checkboxTabs":
                            corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString())).FirstOrDefault();
                            if (corresponding_field == null)
                                break;
                            tab["selected"] = corresponding_field.Value;
                            break;
                        default:
                            corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString())).FirstOrDefault();
                            if (corresponding_field == null)
                                break;
                            tab["value"] = corresponding_field.Value;
                            break;
                    }
                }
            }

            return jobj;
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
                Fr8DisplayType = GetFieldType(tabName),
                Type = tabName.Replace("Tabs", "").UppercaseFirst(), // "textTabs" -> "Text"
                RoleName = roleName,
                TabName = tabName,
                TabLabel = tab["tabLabel"]
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
                        DocumentId = Convert.ToInt32(grpTab["documentId"]),
                        RecipientId = grpTab["recipientId"],
                        Name = string.Format("{0}({1})", grpTab["groupName"], roleName),
                        TabId = grpTab["tabId"],
                        Fr8DisplayType = ControlTypes.RadioButtonGroup,
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
                        Fr8DisplayType = ControlTypes.DropDownList,
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