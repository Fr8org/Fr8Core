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

namespace terminalFr8Core.Actions
{
    public class QueryFr8Warehouse_v1 : BaseTerminalActivity
    {
        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownList AvailableObjects { get; set; }

            [JsonIgnore]
            public FilterPane Filter { get; set; }

            [JsonIgnore]
            public RadioButtonGroup QueryPicker { get; set; }

            public UpstreamCrateChooser UpstreamCrateChooser { get; set; }

            public ActionUi()
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
                                                Source = new FieldSourceDTO(
                                                    CrateManifestTypes.StandardDesignTimeFields,
                                                    "Upstream Crate ManifestType List"
                                                )
                                            },
                                            Label = new DropDownList()
                                            {
                                                Name = "UpstreamCrateLabelDdl",
                                                Source = new FieldSourceDTO(
                                                    CrateManifestTypes.StandardDesignTimeFields,
                                                    "Upstream Crate Label List"
                                                )
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
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "Queryable Objects",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    }
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
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var objectList = GetObjects();

            var upstreamQueryCrateManifests = GetUpstreamCrateManifestListCrate();
            var upstreamQueryCrateLabels = await ExtractUpstreamQueryCrates(curActivityDO);

            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(PackControls(new ActionUi()));
                crateStorage.Add(upstreamQueryCrateManifests);
                crateStorage.Add(upstreamQueryCrateLabels);
                crateStorage.Add(Crate.CreateDesignTimeFieldsCrate("Queryable Objects", objectList.ToArray()));
                crateStorage.Add(
                    Data.Crates.Crate.FromContent(
                        "Found MT Objects",
                        new StandardDesignTimeFieldsCM(
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

        private Crate<StandardDesignTimeFieldsCM> GetUpstreamCrateManifestListCrate()
        {
            var fields = new List<FieldDTO>()
            {
                new FieldDTO(
                    MT.StandardQueryCrate.ToString(),
                    ((int)MT.StandardQueryCrate).ToString(CultureInfo.InvariantCulture)
                )
            };

            var crate = Crate.CreateDesignTimeFieldsCrate("Upstream Crate ManifestType List", fields);

            return crate;
        }

        private async Task<Crate<StandardDesignTimeFieldsCM>>
            ExtractUpstreamQueryCrates(ActivityDO activityDO)
        {
            var upstreamCrates = await GetCratesByDirection<StandardQueryCM>(
                activityDO,
                CrateDirection.Upstream
            );

            var fields = upstreamCrates
                .Select(x => new FieldDTO() { Key = x.Label, Value = x.Label })
                .ToList();

            var crate = Crate.CreateDesignTimeFieldsCrate("Upstream Crate Label List", fields);

            return crate;
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                var ui = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

                if (ui == null)
                {
                    throw new InvalidOperationException("Action was not configured correctly");
                }

                var config = new ActionUi();
                config.ClonePropertiesFrom(ui);
                int selectedObjectId;

                crateStorage.RemoveByLabel("Queryable Criteria");

                if (int.TryParse(config.AvailableObjects.Value, out selectedObjectId))
                {
                    crateStorage.Add(Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", GetFieldsByObjectId(selectedObjectId).ToArray()));
                }
            }
            return Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            var payloadCrateStorage = Crate.GetStorage(payload);

            var ui = GetConfigurationControls(curActivityDO);

            if (ui == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var queryPicker = ui.FindByName<RadioButtonGroup>("QueryPicker");

            List<FilterConditionDTO> conditions;
            int? selectedObjectId = null;

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
                selectedObjectId = ExtractUpstreamObjectId(query);
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

                int objectId;
                if (!int.TryParse(availableObjects.Value, out objectId))
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

                using (var crateStorage = Crate.GetUpdatableStorage(payload))
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
                var obj = uow.MTObjectRepository.GetQuery().FirstOrDefault(x => x.Id == objectId);

                if (obj == null)
                {
                    return Error(payload, "Invalid object selected");
                }

                Type manifestType;
                if (!ManifestDiscovery.Default.TryResolveType(new CrateManifestType(null, obj.ManifestId), out manifestType))
                {
                    return Error(payload, string.Format("Unknown manifest id: {0}", obj.ManifestId));
                }

                var queryBuilder = MTSearchHelper.CreateQueryProvider(manifestType);
                var converter = CrateManifestToRowConverter(manifestType);
                var foundObjects = queryBuilder.Query(
                    uow,
                    authTokenDO.UserID,
                    // Currently MT supports only EQ and NEQ operators, since all fields are strings.
                    conditions.Where(x => x.Operator == "eq" || x.Operator == "neq").ToList()
                )
                .ToArray();

                var searchResult = new StandardPayloadDataCM();

                foreach (var foundObject in foundObjects)
                {
                    searchResult.PayloadObjects.Add(converter(foundObject));
                }

                using (var crateStorage = Crate.GetUpdatableStorage(payload))
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

        private int? ExtractUpstreamObjectId(
            QueryDTO query)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var obj = uow.MTObjectRepository
                    .GetQuery()
                    .FirstOrDefault(x => x.Name == query.Name);

                if (obj == null)
                {
                    return null;
                }

                return obj.Id;
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

        private IEnumerable<FieldDTO> GetFieldsByObjectId(int objectId)
        {
            var fields = new Dictionary<string, string>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var field in uow.MTFieldRepository.GetQuery().Where(x => x.MT_ObjectId == objectId))
                {
                    var alias = "Value" + field.FieldColumnOffset;
                    string existingAlias;

                    if (fields.TryGetValue(field.Name, out existingAlias))
                    {
                        if (existingAlias != alias)
                        {
                            throw new InvalidOperationException(string.Format("Duplicate field definition. MT object type: {0}. Field {1} is mapped to {2} and {3}", objectId, field.Name, existingAlias, alias));
                        }
                    }
                    else
                    {
                        fields[field.Name] = alias;
                    }
                }
            }

            return fields.OrderBy(x => x.Key).Select(x => new FieldDTO(x.Key, x.Key));
        }

        private IEnumerable<FieldDTO> GetObjects()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.MTObjectRepository.GetAll().Select(c => new FieldDTO(c.Name, c.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}