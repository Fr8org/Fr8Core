using Data.Interfaces;
using Data.Constants;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using StructureMap;
using System;
using System.Collections.Generic;
using Utilities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static CrateDTO CrateDTO1()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Label 1",
                Contents = "Contents 1",
                ParentCrateId = ""
            };
        }

        public static CrateDTO CrateDTO2()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Label 2",
                Contents = "Contents 2",
                ParentCrateId = ""
            };
        }

        public static CrateDTO CrateDTO3()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Test",
                Contents = "Container Created Test",
                ParentCrateId = ""
            };
        }

        public static List<ControlDefinitionDTO> SampleConfigurationControls()
        {
            var fieldSelectDocusignTemplateDTO = new DropDownListControlDefinitionDTO()
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
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var recipientSource = new RadioButtonGroupControlDefinitionDTO()
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

            recipientSource.Radios[0].Controls.Add(new TextBoxControlDefinitionDTO()
            {
                Label = "",
                Name = "Address"
            });

            recipientSource.Radios[1].Controls.Add(new DropDownListControlDefinitionDTO()
            {
                Label = "",
                Name = "Select Upstream Crate",
                Source = new FieldSourceDTO
                {
                    Label = "Upstream Plugin-Provided Fields",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
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
            var fieldSelectDocusignTemplateDTO = new DropDownListControlDefinitionDTO()
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
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var recipientSource = new RadioButtonGroupControlDefinitionDTO()
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

            recipientSource.Radios[0].Controls.Add(new DropDownListControlDefinitionDTO()
            {
                Label = "",
                Name = "Select Upstream Crate",
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Upstream Plugin-Provided Fields",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            });

            var textboxControl = new TextBoxControlDefinitionDTO()
            {
                Label = "Address",
                Name = "Address",
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                }
            };

            var textblockControl = new TextBlockControlDefinitionDTO()
            {
                Label = "Docu Sign Envelope",
                Value = "This Action doesn't require any configuration.",
                CssClass = "well well-lg"
            };

            var filepickerControl = new FilePickerControlDefinitionDTO()
            {
                Label = "Select a File"
            };

            var mapingPaneControl = new MappingPaneControlDefinitionDTO()
            {
                Label = "Mapping Pane"
            };

            var fieldFilterPane = new FilterPaneControlDefinitionDTO()
            {
                Label = "Execute Actions If:",
                Name = "Selected_Filter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            var mappingPane = new MappingPaneControlDefinitionDTO()
            {
                Label = "Configure Mapping",
                Name = "Selected_Mapping",
                Required = true
            };

            var checkboxControl = new CheckBoxControlDefinitionDTO()
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
                mappingPane,
                checkboxControl
            };

            return fieldsDTO;
        }

        public static CrateDTO CrateDTOForTwilioConfiguration()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Configuration_Controls",
                Contents = "{\"Controls\":[{\"groupName\":\"SMSNumber_Group\",\"radios\":[{\"selected\":false,\"name\":\"SMSNumberOption\",\"value\":null,\"controls\":[{\"name\":\"SMS_Number\",\"required\":true,\"value\":null,\"label\":\"SMS Number\",\"type\":\"TextBox\",\"selected\":false,\"events\":null,\"source\":null}]},{\"selected\":true,\"name\":\"SMSNumberOption\",\"value\":null,\"controls\":[{\"listItems\":[{\"Key\":\"+15005550006\",\"Value\":\"+15005550006\"}],\"name\":\"upstream_crate\",\"required\":false,\"value\":\"+15005550006\",\"label\":\"a value from Upstream Crate:\",\"type\":\"DropDownList\",\"selected\":false,\"events\":[{\"name\":\"onChange\",\"handler\":\"requestConfig\"}],\"source\":{\"manifestType\":\"Standard Design-Time Fields\",\"label\":\"Available Fields\"}}]}],\"name\":null,\"required\":false,\"value\":null,\"label\":\"For the SMS Number use:\",\"type\":\"RadioButtonGroup\",\"selected\":false,\"events\":null,\"source\":null},{\"name\":\"SMS_Body\",\"required\":true,\"value\":\"DocuSign Sent\",\"label\":\"SMS Body\",\"type\":\"TextBox\",\"selected\":false,\"events\":null,\"source\":null}]}",
            };
        }

    }
}
