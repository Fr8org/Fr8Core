using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Data.Entities;
using StructureMap;
using Hub.Managers;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.States;
using terminalFr8Core.Infrastructure;
using TerminalBase.Services;
using TerminalBase.Services.MT;

namespace terminalFr8Core.Actions
{
    public class QueryFr8Warehouse_v1 : BaseTerminalActivity
    {
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

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configurationCrate = PackControls(new ActivityUi());
            FillObjectsSource(configurationCrate, "AvailableObjects");
            FillUpstreamCrateManifestTypeDDLSource(configurationCrate);
            await FillUpstreamCrateLabelDDLSource(configurationCrate, curActivityDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(configurationCrate);             
                crateStorage.Add(
                    Data.Crates.Crate.FromContent(
                        "Found MT Objects",
                        new FieldDescriptionsCM(
                            new FieldDTO
                            {
                                Key = "Found MT Objects",
                                Value = "Table",
                                Availability = AvailabilityType.RunTime
                            }
                        )
                    )
                );
            }

            return curActivityDO;
        }

        /*private async Task<Crate<FieldDescriptionsCM>>
            ExtractUpstreamQueryCrates(ActivityDO activityDO)
        {
            var upstreamCrates = await GetCratesByDirection<StandardQueryCM>(
                activityDO,
                CrateDirection.Upstream
            );

            var fields = upstreamCrates
                .Select(x => new FieldDTO() { Key = x.Label, Value = x.Label })
                .ToList();

            var crate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Crate Label List", fields);

            return crate;
        }*/

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var ui = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

                if (ui == null)
                {
                    throw new InvalidOperationException("Action was not configured correctly");
                }

                var config = new ActivityUi();
                config.ClonePropertiesFrom(ui);
                Guid selectedObjectId;

                crateStorage.RemoveByLabel("Queryable Criteria");

                if (Guid.TryParse(config.AvailableObjects.Value, out selectedObjectId))
                {
                    // crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", GetFieldsByTypeId(selectedObjectId).ToArray()));
                    crateStorage.Add(
                        Crate.FromContent(
                            "Queryable Criteria",
                            new FieldDescriptionsCM(MTTypesHelper.GetFieldsByTypeId(selectedObjectId))
                        )
                    );
                }
            }

            return Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            var payloadCrateStorage = CrateManager.GetStorage(payload);

            var ui = GetConfigurationControls(curActivityDO);

            if (ui == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var queryPicker = ui.FindByName<RadioButtonGroup>("QueryPicker");

            List<FilterConditionDTO> conditions;
            Guid? selectedObjectId = null;

            if (queryPicker.Radios[0].Selected)
            {
                var upstreamCrateChooser = (UpstreamCrateChooser)(queryPicker).Radios[0].Controls[0];

                var queryCM = await ExtractUpstreamQuery(curActivityDO, upstreamCrateChooser);
                if (queryCM == null || queryCM.Queries == null || queryCM.Queries.Count == 0)
                {
                    return Error(payload, "No upstream crate found");
                }

                var query = queryCM.Queries[0];

                conditions = query.Criteria ?? new List<FilterConditionDTO>();
                selectedObjectId = ExtractUpstreamTypeId(query);
            }
            else
            {
                var filterPane = (FilterPane)queryPicker.Radios[1].Controls[1];
                var availableObjects = (DropDownList)queryPicker.Radios[1].Controls[0];
                var criteria = JsonConvert.DeserializeObject<FilterDataDTO>(filterPane.Value);

                if (availableObjects.Value == null)
                {
                    return Error(payload, "This action is designed to query the Fr8 Warehouse for you, but you don't currently have any objects stored there.");
                }

                Guid objectId;
                if (Guid.TryParse(availableObjects.Value, out objectId))
                {
                    selectedObjectId = objectId;
                }

                conditions = (criteria.ExecutionType == FilterExecutionType.WithoutFilter)
                    ? new List<FilterConditionDTO>()
                    : criteria.Conditions;
            }

            // If no object is found in MT database, return empty result.
            if (!selectedObjectId.HasValue)
            {
                var searchResult = new StandardPayloadDataCM();

                using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
                {
                    crateStorage.Add(Data.Crates.Crate.FromContent("Found MT Objects", searchResult));
                }

                return Success(payload);
            }

            //STARTING NASTY CODE
            //TODO discuss this with Alex (bahadir)
            var envIdCondition = conditions.FirstOrDefault(c => c.Field == "EnvelopeId");
            if (envIdCondition != null && envIdCondition.Value == "FromPayload")
            {
                envIdCondition.Value = GetCurrentEnvelopeId(payloadCrateStorage);
            }
            //END OF NASTY CODE

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var objectId = selectedObjectId.GetValueOrDefault();
                var mtType = uow.MultiTenantObjectRepository.FindTypeReference(objectId);

                if (mtType == null)
                {
                    return Error(payload, "Invalid object selected");
                }

                Type manifestType = mtType.ClrType;

                var queryBuilder = MTSearchHelper.CreateQueryProvider(manifestType);
                var converter = CrateManifestToRowConverter(manifestType);
                var foundObjects = queryBuilder.Query(
                    uow,
                    authTokenDO.UserID,
                    conditions.ToList()
                )
                .ToArray();

                var searchResult = new StandardPayloadDataCM();

                foreach (var foundObject in foundObjects)
                {
                    searchResult.PayloadObjects.Add(converter(foundObject));
                }

                using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
                {
                    crateStorage.Add(Data.Crates.Crate.FromContent("Found MT Objects", searchResult));
                }
            }

            return Success(payload);
        }

        private async Task<StandardQueryCM> ExtractUpstreamQuery(
            ActivityDO activityDO,
            UpstreamCrateChooser queryPicker)
        {
            var upstreamQueryCrateLabel = queryPicker.SelectedCrates[0].Label.Value;

            if (string.IsNullOrEmpty(upstreamQueryCrateLabel))
            {
                return null;
            }

            var upstreamQueryCrate =
                (await GetCratesByDirection<StandardQueryCM>(
                    activityDO,
                    CrateDirection.Upstream
                ))
                .FirstOrDefault(x => x.Label == upstreamQueryCrateLabel);

            if (upstreamQueryCrate == null)
            {
                return null;
            }

            return upstreamQueryCrate.Content;
        }

        // This is weird to use query's name as the way to address MT type. 
        // MT type has unique ID that should be used for this reason. Query name is something that is displayed to user. It should not contain any internal data.
        private Guid? ExtractUpstreamTypeId(QueryDTO query)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var type = uow.MultiTenantObjectRepository.ListTypeReferences().FirstOrDefault(x => x.Alias == query.Name);

                if (type == null)
                {
                    return null;
                }

                return type.Id;
            }
        }

        private string GetCurrentEnvelopeId(ICrateStorage storage)
        {
            var envelopePayloadCrate = storage.CrateContentsOfType<StandardPayloadDataCM>(c => c.Label == "DocuSign Envelope Payload Data").Single();
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
                    row.PayloadObject.Add(new FieldDTO(accessor.Key, string.Format(CultureInfo.InvariantCulture, "{0}", accessor.Value.GetValue(x))));
                }

                return row;
            };
        }

        #region Fill Source
        private void FillObjectsSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = GetObjects();
            }
        }

        private List<ListItem> GetObjects()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var listTypeReferences = uow.MultiTenantObjectRepository.ListTypeReferences();
                return listTypeReferences.Select(c => new ListItem() { Key = c.Alias, Value = c.Id.ToString("N") }).ToList();
            }
        }

        private void FillUpstreamCrateManifestTypeDDLSource(Crate configurationCrate)
        {
            var selectedCrateDetails = GetSelectedCrateDetails(configurationCrate);
            var control = selectedCrateDetails.ManifestType;
            control.ListItems = GetUpstreamCrateManifestList();
        }

        private CrateDetails GetSelectedCrateDetails(Crate configurationCrate)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var upstreamCrateChooser = configurationControl.FindByNameNested<UpstreamCrateChooser>("UpstreamCrateChooser");
            return upstreamCrateChooser.SelectedCrates.First();
        }

        private List<ListItem> GetUpstreamCrateManifestList()
        {
            var fields = new List<FieldDTO>()
            {
                new FieldDTO(
                    MT.StandardQueryCrate.ToString(),
                    ((int)MT.StandardQueryCrate).ToString(CultureInfo.InvariantCulture)
                )
            };
            return fields.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
        }

        private async Task FillUpstreamCrateLabelDDLSource(Crate configurationCrate, ActivityDO activityDO)
        {
            var selectedCrateDetails = GetSelectedCrateDetails(configurationCrate);
            var control = selectedCrateDetails.Label;
            control.ListItems = await GetExtractUpstreamQueryList(activityDO);
        }

        private async Task<List<ListItem>> GetExtractUpstreamQueryList(ActivityDO activityDO)
        {
            var upstreamCrates = await GetCratesByDirection<StandardQueryCM>(
                activityDO,
                CrateDirection.Upstream
            );

            return upstreamCrates
                 .Select(x => new ListItem() { Key = x.Label, Value = x.Label })
                 .ToList();
        }

        #endregion
    }
}