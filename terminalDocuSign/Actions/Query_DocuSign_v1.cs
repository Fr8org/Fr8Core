using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Query_DocuSign_v1  : BaseTerminalAction
    {
        public enum SearchType
        {
            ByTemplate, 
            ByRecipient
        }
        
        public class RuntimeConfiguration : Manifest
        {
            public SearchType SearchType;
            public string RecipientName;
            public string SelectedTemplate;
            public FilterConditionDTO[] Criteria;

            public RuntimeConfiguration()
                : base(new CrateManifestType("Query_DocuSign_v1_RuntimeConfiguration", 1000000 + 1))
            {
            }
        }

        private class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public TextBox RecipientName { get; set; }

            [JsonIgnore]
            public RadioButtonOption SearchWithRecipient { get; set; }

            [JsonIgnore]
            public RadioButtonOption WithTemplate { get; set; }

            [JsonIgnore]
            public DropDownList Template { get; set; }

            [JsonIgnore]
            public QueryBuilder QueryBuilder { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new RadioButtonGroup
                {
                    Label = "Subject of this Report is Envelopes:",
                    GroupName = "TemplateRecipientPicker",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig},
                    Radios = new List<RadioButtonOption>
                    {
                        (SearchWithRecipient = new RadioButtonOption
                        {
                            Name = "SearchWithRecipient",
                            Value = "Sent to a specific Recipient",
                            Bindings = new [] { "Selected <> SearchWithRecipient"},
                            Controls = new List<ControlDefinitionDTO>
                            {
                                (RecipientName = new TextBox
                                {
                                    Name = "RecipientName",
                                    Bindings = new [] { "Value <> RecipientName"},
                                    Events = new List<ControlEvent> { ControlEvent.RequestConfig},
                                })
                            }
                        }),

                        (WithTemplate = new RadioButtonOption
                        {
                            Name = "WithTemplate",
                            Value = "Sent with a specific Template",
                            Bindings = new [] { "Selected <> SearchWithTemplate"},
                            Controls = new List<ControlDefinitionDTO> 
                            {
                                (Template = new DropDownList
                                {
                                    Name = "Template",
                                    Bindings = new [] { "Value <> SelectedTemplate"},
                                    Events = new List<ControlEvent> { ControlEvent.RequestConfig},
                                    Source = new FieldSourceDTO( CrateManifestTypes.StandardDesignTimeFields, "Available Templates")
                                })
                            }
                        })
                    }
                });

                Controls.Add((QueryBuilder = new QueryBuilder
                {
                    Name = "QueryBuilder",
                    Label = "Additional Search Criteria",
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Criteria",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                }));
            }
        }

        private readonly DocuSignManager _docuSignManager;

        static Query_DocuSign_v1()
        {
            ManifestDiscovery.Default.RegisterManifest(typeof(RuntimeConfiguration));
        }

        public Query_DocuSign_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Runtime Configuration", new RuntimeConfiguration()));
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(PackCrate_DocuSignTemplateNames(docuSignAuthDto));
            }
            
            return Task.FromResult(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                var ui = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (ui == null)
                {
                    updater.DiscardChanges();
                    return curActionDO;
                }

                var controls = new ActionUi();
               
                controls.ClonePropertiesFrom(ui);

                var config = updater.CrateStorage.CrateContentsOfType<RuntimeConfiguration>().First();

                config.RecipientName = controls.RecipientName.Value;
                config.SearchType = controls.SearchWithRecipient.Selected ? SearchType.ByRecipient : SearchType.ByTemplate;
                config.SelectedTemplate = controls.Template.Value;
                config.Criteria = JsonConvert.DeserializeObject<FilterConditionDTO[]>(controls.QueryBuilder.Value);
              
                updater.CrateStorage.RemoveByLabel("Queryable Criteria");

                switch (config.SearchType)
                {
                    case SearchType.ByTemplate:
                        var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);
                        var crate = _docuSignManager.CrateCrateFromFields(config.SelectedTemplate, docuSignAuthDto, "Queryable Criteria");

                        if (crate != null)
                        {
                            updater.CrateStorage.Add(crate);
                        }

                        break;
                }

                return curActionDO;
            }
        }
        
        protected Crate PackCrate_DocuSignTemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();
            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate("Available Templates", fields);
            
            return createDesignTimeFields;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}