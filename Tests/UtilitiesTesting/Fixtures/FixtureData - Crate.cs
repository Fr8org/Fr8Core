using Core.Interfaces;
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

        public static List<ControlDefinitionDTO> SampleConfigurationControls()
        {
            var fieldSelectDocusignTemplateDTO = new DropDownListControlDefinitionDTO()
            {
                Label = "target_docusign_template",
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
                Label = "target_docusign_template",
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

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                recipientSource,
                textboxControl
            };

            return fieldsDTO;
        }

    }
}
