using DocuSign.eSign.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using terminalDocuSign.DataTransferObjects;

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

        public static IEnumerable<KeyValueDTO> GetEnvelopeTabsPerSigner(JObject tabs, string roleName)
        {
            return MapTabsToFieldDTO(ExtractTabs(tabs, roleName));
        }

        public static IEnumerable<KeyValueDTO> MapTabsToFieldDTO(IEnumerable<DocuSignTabDTO> tabs)
        {
            var result = new List<KeyValueDTO>();

            foreach (var tab in tabs)
            {
                if (tab is DocuSignTabDTO)
                {
                    result.Add(new KeyValueDTO(tab.Name, tab.Value) { Tags = string.Format("DocuSignTab:{0}, recipientId:{1}", tab.TabName, tab.RecipientId) });
                }
                else
                    if (tab is DocuSignMultipleOptionsTabDTO)
                {
                    var value = (tab as DocuSignMultipleOptionsTabDTO).Items.Where(a => a.Selected).FirstOrDefault();
                    result.Add(
                        new KeyValueDTO()
                        {
                            Key = tab.Name,
                            Value = value?.Value,
                            Tags = string.Format("DocuSignTab:{0}, recipientId:{1}", tab.TabName, tab.RecipientId)
                        });
                }
            }
            return result;
        }

        public static JObject ApplyValuesToTabs(List<KeyValueDTO> fieldList, Signer corresponding_template_recipient, Tabs tabs)
        {
            JObject jobj = JObject.Parse(tabs.ToJson());
            foreach (var item in jobj.Properties())
            {
                string tab_type = item.Name;
                var fields = fieldList.Where(a => a.Tags.Contains(tab_type) && a.Tags.Contains("recipientId:" + corresponding_template_recipient.RecipientId)).ToArray();

                foreach (JObject tab in item.Value)
                {
                    KeyValueDTO corresponding_field = null;
                    switch (tab_type)
                    {
                        case "radioGroupTabs":
                            corresponding_field = fields.FirstOrDefault(a => a.Key.Contains(tab.Property("groupName").Value.ToString()));
                            if (corresponding_field == null)
                                break;
                            foreach (var radioItem in tab["radios"])
                            {
                                radioItem["selected"] = radioItem["value"].ToString() == corresponding_field.Value ? "true" : "false";
                            }
                            break;

                        case "listTabs":
                            corresponding_field = fields.FirstOrDefault(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString()));
                            if (corresponding_field == null)
                                break;
                            var trimmedValue = corresponding_field.Value?.Trim();
                            foreach (var listItem in tab["listItems"])
                            {
                                listItem["selected"] = listItem["value"].ToString() == trimmedValue ? "true" : "false";
                            }
                            tab["value"] = corresponding_field.Value;
                            break;
                        case "checkboxTabs":
                            corresponding_field = fields.FirstOrDefault(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString()));
                            if (corresponding_field == null)
                                break;
                            tab["selected"] = corresponding_field.Value;
                            break;
                        default:
                            corresponding_field = fields.FirstOrDefault(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString()));
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
                Name = tab["tabLabel"] + (roleName.ToStr().IsNullOrEmpty() ? null : $" ({roleName})"),
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
                        Name = grpTab["groupName"] + (roleName.ToStr().IsNullOrEmpty() ? null : $" ({roleName})"),
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
                        Name = grpTab["tabLabel"] + (roleName.ToStr().IsNullOrEmpty() ? null : $" ({roleName})"),
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

                            var item = new DocuSignOptionItemTabDTO
                            {
                                Text = lstItem["text"],
                                Value = lstItem["value"],
                                Selected = lstItem["selected"]
                            };

                            if (string.IsNullOrEmpty(item.Text) && string.IsNullOrEmpty(item.Value))
                            {
                                item.Text = " ";  //DS allows to select empty value in DDLB
                                item.Value = " ";
                            }
                            groupWrapperEnvelopeData.Items.Add(item);
                        }
                    }

                    yield return groupWrapperEnvelopeData;
                }
            }
        }

        #endregion
    }
}