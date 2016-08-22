using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Hub.Services;
using Hub.Services.MT;
using Newtonsoft.Json;
using StructureMap;


namespace terminalFr8Core.Activities
{
    public class Query_Fr8_Warehouse_v1 : ExplicitTerminalActivity
    {
        private readonly IContainer _container;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("ad46fa79-eb0b-4990-ad01-76ebf9d471da"),
            Name = "Query_Fr8_Warehouse",
            Label = "Query Fr8 Warehouse",
            Version = "1",
            MinPaneWidth = 550,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownList AvailableObjects { get; set; }

            [JsonIgnore]
            public FilterPane Filter { get; set; }

            [JsonIgnore]
            public RadioButtonGroup QueryPicker { get; set; }

            public UpstreamCrateChooser UpstreamCrateChooser { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(QueryPicker = new RadioButtonGroup()
                {
                    Label = "Select query to use:",
                    GroupName = "QueryPickerGroup",
                    Name = "QueryPicker",
                    Radios = new List<RadioButtonOption>()
                    {
                        new RadioButtonOption()
                        {
                            Selected = true,
                            Name = "ExistingQuery",
                            Value = "Use existing Query",
                            Controls = new List<ControlDefinitionDTO>()
                            {
                                (UpstreamCrateChooser = new UpstreamCrateChooser()
                                {
                                    Name = "UpstreamCrateChooser",
                                    SelectedCrates = new List<CrateDetails>()
                                    {
                                        new CrateDetails()
                                        {
                                            ManifestType = new DropDownList()
                                            {
                                                Name = "UpstreamCrateManifestTypeDdl",
                                                Source = null
                                            },
                                            Label = new DropDownList()
                                            {
                                                Name = "UpstreamCrateLabelDdl",
                                                Source = null
                                            }
                                        }
                                    },
                                    MultiSelection = false
                                })
                            }
                        },

                        new RadioButtonOption()
                        {
                            Selected = false,
                            Name = "NewQuery",
                            Value = "Use new Query",
                            Controls = new List<ControlDefinitionDTO>()
                            {
                                (AvailableObjects = new DropDownList
                                {
                                    Label = "Object List",
                                    Name = "AvailableObjects",
                                    Value = null,
                                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                                    Source = null
                                }),
                                (Filter = new FilterPane
                                {
                                    Label = "Find all Fields where:",
                                    Name = "Filter",
                                    Required = true,
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "Queryable Criteria",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    }
                                })
                            }
                        }
                    }
                });
            }
        }

        /*private async Task<StandardQueryCM> ExtractUpstreamQuery(UpstreamCrateChooser queryPicker)
        {
            var upstreamQueryCrateLabel = queryPicker.SelectedCrates[0].Label.Value;
            if (string.IsNullOrEmpty(upstreamQueryCrateLabel))
            {
                return null;
            }
            var upstreamQueryCrate = (await HubCommunicator.GetCratesByDirection<StandardQueryCM>(ActivityId, CrateDirection.Upstream))
                .FirstOrDefault(x => x.Label == upstreamQueryCrateLabel);

            return upstreamQueryCrate?.Content;
        }*/

        // This is weird to use query's name as the way to address MT type. 
        // MT type has unique ID that should be used for this reason. Query name is something that is displayed to user. It should not contain any internal data.
        private Guid? ExtractUpstreamTypeId(QueryDTO query)
        {
            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                var type = uow.MultiTenantObjectRepository.ListTypeReferences().FirstOrDefault(x => x.Alias == query.Name);
                return type?.Id;
            }
        }

        private string GetCurrentEnvelopeId()
        {
            var envelopePayloadCrate = Payload.CrateContentsOfType<StandardPayloadDataCM>(c => c.Label == "DocuSign Envelope Fields").Single();
            var envelopeId = envelopePayloadCrate.PayloadObjects.SelectMany(o => o.PayloadObject).Single(po => po.Key == "EnvelopeId").Value;
            return envelopeId;
        }

        private Func<object, PayloadObjectDTO> CrateManifestToRowConverter(Type manifestType)
        {
            var accessors = new List<KeyValuePair<string, IMemberAccessor>>();

            foreach (var member in manifestType.GetMembers(BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.Name))
            {
                IMemberAccessor accessor;

                if (member is FieldInfo)
                {
                    accessor = ((FieldInfo)member).ToMemberAccessor();
                }
                else if (member is PropertyInfo && !((PropertyInfo)member).IsSpecialName)
                {
                    accessor = ((PropertyInfo)member).ToMemberAccessor();
                }
                else
                {
                    continue;
                }

                accessors.Add(new KeyValuePair<string, IMemberAccessor>(member.Name, accessor));
            }

            return x =>
            {
                var row = new PayloadObjectDTO();

                foreach (var accessor in accessors)
                {
                    row.PayloadObject.Add(new KeyValueDTO(accessor.Key, string.Format(CultureInfo.InvariantCulture, "{0}", accessor.Value.GetValue(x))));
                }

                return row;
            };
        }

        #region Fill Source
        private void FillObjectsSource(string controlName)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);

            if (control != null)
            {
                control.ListItems = GetObjects();
            }
        }

        private List<ListItem> GetObjects()
        {
            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                var listTypeReferences = uow.MultiTenantObjectRepository.ListTypeReferences();
                return listTypeReferences.Select(c => new ListItem() { Key = c.Alias, Value = c.Id.ToString("N") }).ToList();
            }
        }

        private void FillUpstreamCrateManifestTypeDDLSource()
        {
            var selectedCrateDetails = GetSelectedCrateDetails();
            var control = selectedCrateDetails.ManifestType;
            control.ListItems = GetUpstreamCrateManifestList();
        }

        private CrateDetails GetSelectedCrateDetails()
        {
            var upstreamCrateChooser = ConfigurationControls.FindByNameNested<UpstreamCrateChooser>("UpstreamCrateChooser");
            return upstreamCrateChooser.SelectedCrates.First();
        }

        private List<ListItem> GetUpstreamCrateManifestList()
        {
            return new List<ListItem>
            {
                new ListItem
                {
                    Key = MT.StandardQueryCrate.ToString(),
                    Value = ((int)MT.StandardQueryCrate).ToString(CultureInfo.InvariantCulture)
                }
                
            };
        }

        /*private async Task FillUpstreamCrateLabelDDLSource(Crate configurationCrate)
        {
            var selectedCrateDetails = GetSelectedCrateDetails(configurationCrate);
            var control = selectedCrateDetails.Label;
            control.ListItems = await GetExtractUpstreamQueryList();
        }*/
        /*
        private async Task<List<ListItem>> GetExtractUpstreamQueryList()
        {
            var upstreamCrates = await HubCommunicator.GetCratesByDirection<StandardQueryCM>(ActivityId, CrateDirection.Upstream);
            return upstreamCrates
                 .Select(x => new ListItem() { Key = x.Label, Value = x.Label })
                 .ToList();
        }
        */
        #endregion

        public Query_Fr8_Warehouse_v1(ICrateManager crateManager, IContainer container)
            : base(crateManager)
        {
            _container = container;
        }

        public override async Task Run()
        {
            var queryPicker = GetControl<RadioButtonGroup>("QueryPicker");
            List<FilterConditionDTO> conditions;
            Guid? selectedObjectId = null;

            if (queryPicker.Radios[0].Selected)
            {
                var upstreamCrateChooser = (UpstreamCrateChooser)(queryPicker).Radios[0].Controls[0];
                var selectedCrate = upstreamCrateChooser.SelectedCrates.FirstOrDefault();
                StandardQueryCM queryCM = null;

                if (selectedCrate != null)
                {
                    queryCM = Payload.CratesOfType<StandardQueryCM>().FirstOrDefault(x => x.Label == selectedCrate.Label.selectedKey)?.Content;
                }

                if (queryCM?.Queries == null || queryCM.Queries.Count == 0)
                {
                    RaiseError("No upstream crate found");
                    return;
                }

                var query = queryCM.Queries[0];

                conditions = query.Criteria ?? new List<FilterConditionDTO>();
                selectedObjectId = ExtractUpstreamTypeId(query);
            }
            else
            {
                var filterPane = (FilterPane)queryPicker.Radios[1].Controls[1];
                var availableObjects = (DropDownList)queryPicker.Radios[1].Controls[0];
                var criteria = filterPane.Value == null ? null : JsonConvert.DeserializeObject<FilterDataDTO>(filterPane.Value);

                if (availableObjects.Value == null)
                {
                    RaiseError("This action is designed to query the Fr8 Warehouse for you, but you don't currently have any objects stored there.");
                    return;
                }

                Guid objectId;
                if (Guid.TryParse(availableObjects.Value, out objectId))
                {
                    selectedObjectId = objectId;
                }

                conditions = criteria == null || criteria.ExecutionType == FilterExecutionType.WithoutFilter
                    ? new List<FilterConditionDTO>()
                    : criteria.Conditions;
            }

            // If no object is found in MT database, return empty result.
            if (!selectedObjectId.HasValue)
            {
                var searchResult = new StandardPayloadDataCM();
                Payload.Add(Crate.FromContent("Found MT Objects", searchResult));
                Success();
                return;
            }

            //STARTING NASTY CODE
            //TODO discuss this with Alex (bahadir)
            var envIdCondition = conditions.FirstOrDefault(c => c.Field == "EnvelopeId");
            if (envIdCondition != null && envIdCondition.Value == "FromPayload")
            {
                envIdCondition.Value = GetCurrentEnvelopeId();
            }
            //END OF NASTY CODE

            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                var objectId = selectedObjectId.GetValueOrDefault();
                var mtType = uow.MultiTenantObjectRepository.FindTypeReference(objectId);

                if (mtType == null)
                {
                    RaiseError("Invalid object selected");
                    return;
                }
                Type manifestType = mtType.ClrType;
                var queryBuilder = MTSearchHelper.CreateQueryProvider(manifestType);
                var converter = CrateManifestToRowConverter(manifestType);
                var foundObjects = queryBuilder.Query(uow,CurrentUserId,conditions.ToList()).ToArray();
                var searchResult = new StandardPayloadDataCM();
                foreach (var foundObject in foundObjects)
                {
                    searchResult.PayloadObjects.Add(converter(foundObject));
                }
                Payload.Add(Crate.FromContent("Found MT Objects", searchResult));
            }

            Success();
        }

        public override async Task Initialize()
        {
            AddControls(new ActivityUi().Controls);
            
            FillObjectsSource("AvailableObjects");
            FillUpstreamCrateManifestTypeDDLSource();
           // await FillUpstreamCrateLabelDDLSource(configurationCrate);

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>("Found MT Objects");
        }

        public override Task FollowUp()
        {
            var config = new ActivityUi();
            config.ClonePropertiesFrom(ConfigurationControls);
            Guid selectedObjectId;
            Storage.RemoveByLabel("Queryable Criteria");
            if (Guid.TryParse(config.AvailableObjects.Value, out selectedObjectId))
            {
                using (var uow = _container.GetInstance<IUnitOfWork>())
                {
                    // crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", GetFieldsByTypeId(selectedObjectId).ToArray()));
                    Storage.Add(
                        Crate.FromContent("Queryable Criteria",
                            new FieldDescriptionsCM(MTTypesHelper.GetFieldsByTypeId(uow, selectedObjectId))
                            )
                        );
                }
            }
            return Task.FromResult(0);
        }
    }
}