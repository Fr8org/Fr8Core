using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Newtonsoft.Json;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Utilities;
using System.Configuration;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
//        public static Crate CrateDTO1()
//        {
//            return new Crate()
//            {
//                Id = Guid.NewGuid().ToString(),
//                Label = "Label 1",
//                Contents = "Contents 1",
//                ParentCrateId = ""
//            };
//        }
//
//        public static CrateDTO CrateDTO2()
//        {
//            return new CrateDTO()
//            {
//                Id = Guid.NewGuid().ToString(),
//                Label = "Label 2",
//                Contents = "Contents 2",
//                ParentCrateId = ""
//            };
//        }
//
//        public static CrateDTO CrateDTO3()
//        {
//            return new CrateDTO()
//            {
//                Id = Guid.NewGuid().ToString(),
//                Label = "Test",
//                Contents = "Container Created Test",
//                ParentCrateId = ""
//            };
//        }

        private static string phoneNumber = ConfigurationManager.AppSettings["TestPhoneNumber"];
        public static List<ControlDefinitionDTO> SampleConfigurationControls()
        {
            var fieldSelectDocusignTemplateDTO = new DropDownList()
            {
                Label = "Use DocuSign Template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.FieldDescription.GetEnumDisplayName()
                }
            };

            var recipientSource = new RadioButtonGroup()
            {
                Label = "Recipient",
                GroupName = "Recipient",
                Name = "Recipient",
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "specific",
                        Value ="This specific value"
                    },
                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "crate",
                        Value ="A value from an Upstream Crate"
                    }
                }
            };

            recipientSource.Radios[0].Controls.Add(new TextBox()
            {
                Label = "",
                Name = "Address"
            });

            recipientSource.Radios[1].Controls.Add(new DropDownList()
            {
                Label = "",
                Name = "Select Upstream Crate",
                Source = new FieldSourceDTO
                {
                    Label = "Upstream Terminal-Provided Fields",
                    ManifestType = MT.FieldDescription.GetEnumDisplayName()
                }
            });

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                recipientSource
            };

            return fieldsDTO;
        }

        public static List<ControlDefinitionDTO> AllConfigurationControls()
        {
            var fieldSelectDocusignTemplateDTO = new DropDownList()
            {
                Label = "Use DocuSign Template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                },
                ListItems = new List<ListItem>()
                {
                    new ListItem()
                    {
                        Key = "Item key 1",
                        Value = "Item value 1",
                        Selected = true
                    },
                    new ListItem()
                    {
                        Key = "Item key 2",
                        Value = "Item value 2"
                    },
                    new ListItem()
                    {
                        Key = "Item key 3",
                        Value = "Item value 3"
                    }
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.FieldDescription.GetEnumDisplayName()
                }
            };

            var recipientSource = new RadioButtonGroup()
            {
                Label = "Recipient",
                GroupName = "Recipient",
                Name = "Recipient",
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "specific",
                        Value ="This specific value"
                    },
                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "crate",
                        Value ="A value from an Upstream Crate"
                    }
                }
            };

            recipientSource.Radios[0].Controls.Add(new DropDownList()
            {
                Label = "",
                Name = "Select Upstream Crate",
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Upstream Terminal-Provided Fields",
                    ManifestType = MT.FieldDescription.GetEnumDisplayName()
                }
            });

            var textboxControl = new TextBox()
            {
                Label = "Address",
                Name = "Address",
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                }
            };

            var textblockControl = new TextBlock()
            {
                Label = "Docu Sign Envelope",
                Value = "This Action doesn't require any configuration.",
                CssClass = "well well-lg"
            };

            var filepickerControl = new FilePicker()
            {
                Label = "Select a File"
            };

            var fieldFilterPane = new FilterPane()
            {
                Label = "Execute Actions If:",
                Name = "Selected_Filter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };
            
            var checkboxControl = new CheckBox()
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent"
            };

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                recipientSource,
                textboxControl,
                textblockControl,
                filepickerControl,
                fieldFilterPane,
                checkboxControl
            };

            return fieldsDTO;
        }

        public static Crate CrateDTOForTwilioConfiguration()
        {
            var controls = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>("{\"Controls\":[{\"groupName\":\"SMSNumber_Group\",\"radios\":[{\"selected\":false,\"name\":\"SMSNumberOption\",\"value\":null,\"controls\":[{\"name\":\"SMS_Number\",\"required\":true,\"value\":null,\"label\":\"SMS Number\",\"type\":\"TextBox\",\"selected\":false,\"events\":null,\"source\":null}]},{\"selected\":true,\"name\":\"SMSNumberOption\",\"value\":null,\"controls\":[{\"listItems\":[{\"Key\":\""+ phoneNumber + "\",\"Value\":\""+ phoneNumber + "\"}],\"name\":\"upstream_crate\",\"required\":false,\"value\":\""+ phoneNumber + "\",\"label\":\"a value from Upstream Crate:\",\"type\":\"DropDownList\",\"selected\":false,\"events\":[{\"name\":\"onChange\",\"handler\":\"requestConfig\"}],\"source\":{\"manifestType\":\"Standard Design-Time Fields\",\"label\":\"Available Fields\"}}]}],\"name\":null,\"required\":false,\"value\":null,\"label\":\"For the SMS Number use:\",\"type\":\"RadioButtonGroup\",\"selected\":false,\"events\":null,\"source\":null},{\"name\":\"SMS_Body\",\"required\":true,\"value\":\"DocuSign Sent\",\"label\":\"SMS Body\",\"type\":\"TextBox\",\"selected\":false,\"events\":null,\"source\":null}]}", new ControlDefinitionDTOConverter());

            return Infrastructure.Data.Crates.Crate.FromContent("Configuration_Controls", controls);
        }
        public static List<LogItemDTO> LogItemDTOList()
        {
            var curLogItemDTOList = new List<LogItemDTO>();

            var curLogItemDTO = new LogItemDTO
            {
                Name = "LogItemDTO1",
                PrimaryCategory = "Container",
                SecondaryCategory = "LogItemDTO Generator",
                Activity = "Add Log Message",
                Data = ""
            };

            curLogItemDTOList.Add(curLogItemDTO);

            return curLogItemDTOList;
        }

        public static CrateDTO CrateDTOForEvents(string externalAccountId)
        {
            EventReportCM curEventReportMS = new EventReportCM();
            curEventReportMS.EventNames = "DocuSign Envelope Sent";
            curEventReportMS.EventPayload.Add(FixtureData.GetEnvelopeIdCrate());
            curEventReportMS.ExternalAccountId = externalAccountId;
            curEventReportMS.Manufacturer = "DocuSign";
            var curEventReport = Crate.FromContent("Standard Event Report", curEventReportMS);
            return new CrateManager().ToDto(curEventReport);
        }
    }
}
