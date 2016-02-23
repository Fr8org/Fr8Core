using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.Repositories;
using Data.States;
using Utilities;
using terminalFr8Core;
using terminalFr8Core.Infrastructure;
using terminalFr8Core.Interfaces;
using terminalFr8Core.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Services;
using System.Text.RegularExpressions;
using Hub.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class SearchFr8Warehouse_v1 : BaseTerminalActivity
    {
        private const string QueryCrateLabel = "Fr8 Search Query";
        private const string SolutionName = "Search Fr8 Warehouse";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "terminalFr8Core";

        // Here in this action we have query builder control to build queries against MT database.
        // Note We are ignoring the generic type searching and fetching  FR-2317

        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public QueryBuilder QueryBuilder { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for Fr8 Warehouse where the following are true:</p>"
                });

                Controls.Add(new DropDownList()
                {
                    Name = "Select MT Objects",
                    Required = true,
                    Label = "Select MT Objects",
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Objects",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                });

                Controls.Add((QueryBuilder = new QueryBuilder
                {
                    Name = "QueryBuilder",
                    Value = null,
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Criteria",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }));

                Controls.Add(new Button()
                {
                    Label = "Continue",
                    Name = "Continue",
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
                });
            }
        }

        public SearchFr8Warehouse_v1()
        {
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            return Success(payload);
        }

        public override async Task<PayloadDTO> ChildrenExecuted(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configurationControls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            // Merge data from QueryMT action.
            var payloadCrateStorage = CrateManager.FromDto(payload.CrateStorage);
            var queryMTResult = payloadCrateStorage
                .CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();

            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                crateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", queryMTResult));
            }

            return ExecuteClientAction(payload, "ShowTableReport");
        }


        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(PackControls(new ActionUi()));
                var designTimefieldLists = ExtractDesignTimeField(authTokenDO);
                var availableMtObjects = CrateManager.CreateDesignTimeFieldsCrate("Queryable Objects", designTimefieldLists.ToArray());
                crateStorage.Add(availableMtObjects);
                crateStorage.AddRange(PackDesignTimeData());
            }
            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            try
            {
                var continueClicked = false;

                using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
                {
                    var controls = crateStorage
                        .CrateContentsOfType<StandardConfigurationControlsCM>()
                        .FirstOrDefault();
                    var mtObjectDropown = controls.FindByName<DropDownList>("Select MT Objects");
                    var mtObject = mtObjectDropown.Value;
                    var continueButton = controls.FindByName<Button>("Continue");
                    var queryBuilder = controls.FindByName<QueryBuilder>("QueryBuilder");

                    if (continueButton != null)
                    {
                        continueClicked = continueButton.Clicked;

                        if (continueButton.Clicked)
                        {
                            continueButton.Clicked = false;
                        }
                    }

                    // Add the logic for getting the RunTime Object properties from the Fr8 Warehouse
                    if (!continueClicked)
                    {
                        var designTimeQueryFields = ExtractQueryDesignTimeFields(mtObject);
                        var criteria = crateStorage.FirstOrDefault(d => d.Label == "Queryable Criteria");

                        if (criteria != null)
                        {
                            crateStorage.Remove(criteria);
                        }
                        crateStorage.Add(Data.Crates.Crate.FromContent("Queryable Criteria", new StandardQueryFieldsCM(designTimeQueryFields)));
                    }
                }


                if (continueClicked)
                {
                    using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
                    {
                        crateStorage.Remove<StandardQueryCM>();
                        var controls = crateStorage
                       .CrateContentsOfType<StandardConfigurationControlsCM>()
                       .FirstOrDefault();
                        var dropDown = controls.FindByName<DropDownList>("Select MT Objects");
                        var mtObject = dropDown.selectedKey;
                        var queryCrate = ExtractQueryCrate(crateStorage, mtObject);
                        crateStorage.Add(queryCrate);
                    }

                    var activityTemplates = (await HubCommunicator.GetActivityTemplates(activityDO, null)).Select(x => Mapper.Map<ActivityTemplateDO>(x)).ToList();
                    activityDO.ChildNodes.Clear();

                    var queryFr8WarehouseActivityTemplate = activityTemplates
                        .FirstOrDefault(x => x.Name == "QueryFr8Warehouse");
                    if (queryFr8WarehouseActivityTemplate == null) { return activityDO; }

                    var queryFr8WarehouseAction = await AddAndConfigureChildActivity(
                        activityDO,
                        "QueryFr8Warehouse"
                    );

                    using (var crateStorage = CrateManager.GetUpdatableStorage(queryFr8WarehouseAction))
                    {
                        crateStorage.RemoveByLabel("Upstream Crate Label List");

                        var fields = new[]
                        {
                            new FieldDTO() { Key = QueryCrateLabel, Value = QueryCrateLabel }
                        };
                        var upstreamLabelsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Crate Label List", fields);
                        crateStorage.Add(upstreamLabelsCrate);

                        var upstreamManifestTypes = crateStorage
                            .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Crate ManifestType List")
                            .FirstOrDefault();

                        var controls = crateStorage
                            .CrateContentsOfType<StandardConfigurationControlsCM>()
                            .FirstOrDefault();

                        var radioButtonGroup = controls
                            .FindByName<RadioButtonGroup>("QueryPicker");

                        UpstreamCrateChooser upstreamCrateChooser = null;
                        if (radioButtonGroup != null
                            && radioButtonGroup.Radios.Count > 0
                            && radioButtonGroup.Radios[0].Controls.Count > 0)
                        {
                            upstreamCrateChooser = radioButtonGroup.Radios[0].Controls[0] as UpstreamCrateChooser;
                        }

                        if (upstreamCrateChooser != null)
                        {
                            upstreamCrateChooser.SelectedCrates[0].ManifestType.selectedKey = upstreamManifestTypes.Fields[0].Key;
                            upstreamCrateChooser.SelectedCrates[0].ManifestType.Value = upstreamManifestTypes.Fields[0].Value;
                            upstreamCrateChooser.SelectedCrates[0].Label.selectedKey = QueryCrateLabel;
                            upstreamCrateChooser.SelectedCrates[0].Label.Value = QueryCrateLabel;
                        }
                    }

                    queryFr8WarehouseAction = await ConfigureChildActivity(
                        activityDO,
                        queryFr8WarehouseAction
                    );

                    using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
                    {
                        crateStorage.RemoveByManifestId((int)MT.OperationalStatus);

                        var operationalStatus = new OperationalStateCM();
                        operationalStatus.CurrentActivityResponse =
                            ActivityResponseDTO.Create(ActivityResponse.ExecuteClientAction);
                        operationalStatus.CurrentClientActionName = "ExecuteAfterConfigure";

                        var operationsCrate = Data.Crates.Crate.FromContent("Operational Status", operationalStatus);
                        crateStorage.Add(operationsCrate);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return activityDO;
        }

        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = new SolutionPageDTO
                {
                    Name = SolutionName,
                    Version = SolutionVersion,
                    Terminal = TerminalName,
                    Body = @"<p>This is Search Fr8 solution action</p>"
                };
                return Task.FromResult(curSolutionPage);
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonce("Unknown displayMechanism: we currently support MainPage cases"));
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            return ConfigurationRequestType.Followup;
        }

        private static ControlDefinitionDTO CreateTextBoxQueryControl(
            string key)
        {
            return new TextBox()
            {
                Name = "QueryField_" + key
            };
        }

        private Crate<StandardQueryCM> ExtractQueryCrate(ICrateStorage storage, string mtObject)
        {
            var configurationControls = storage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .SingleOrDefault();

            if (configurationControls == null)
            {
                throw new ApplicationException("Action was not configured correctly");
            }

            var actionUi = new ActionUi();
            actionUi.ClonePropertiesFrom(configurationControls);

            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                actionUi.QueryBuilder.Value
            );

            var queryCM = new StandardQueryCM(
                new QueryDTO()
                {
                    Name = mtObject,
                    Criteria = criteria
                }
            );

            return Crate<StandardQueryCM>.FromContent(QueryCrateLabel, queryCM);
        }

        private IEnumerable<Crate> PackDesignTimeData()
        {
            yield return Data.Crates.Crate.FromContent("Fr8 Search Report", new StandardDesignTimeFieldsCM(new FieldDTO
            {
                Key = "Fr8 Search Report",
                Value = "Table",
                Availability = AvailabilityType.RunTime
            }));
        }

        // search MT Object into DB
        private MT_Object ExtractMTObject(
           string mtObject)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var obj = uow.MTObjectRepository
                    .GetQuery()
                    .FirstOrDefault(x => x.Name == mtObject);

                if (obj == null)
                {
                    return null;
                }

                return obj;
            }
        }

        // create the dropdown design time fields.
        private List<FieldDTO> ExtractDesignTimeField(AuthorizationTokenDO oAuthToken)
        {
            using (var unitWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curMtObjectResult = unitWork.MTDataRepository.FindList(d => d.fr8AccountId == oAuthToken.UserID);

                var listFieldDTO = new List<FieldDTO>();

                foreach (var item in curMtObjectResult)
                {
                    var mtObjectRepository = unitWork.MTObjectRepository.GetQuery().FirstOrDefault(d => d.Id == item.MT_ObjectId);

                    if (!listFieldDTO.Exists(d => d.Key == mtObjectRepository.Name))
                    {
                        listFieldDTO.Add(new FieldDTO()
                        {
                            Key = mtObjectRepository.Name,
                            Value = mtObjectRepository.Name
                        });
                    }
                }
                return listFieldDTO;
            }
        }

        // create the Query design time fields.
        private List<QueryFieldDTO> ExtractQueryDesignTimeFields(string mtObjectName)
        {
            List<QueryFieldDTO> designTimeQueryFields = new List<QueryFieldDTO>();
            var mtObject = ExtractMTObject(mtObjectName);

            using (var unitWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var field in unitWork.MTFieldRepository.GetQuery().Where(x => x.MT_ObjectId == mtObject.Id))
                {
                    if (!designTimeQueryFields.Exists(d => d.Name == field.Name))
                    {
                        designTimeQueryFields.Add(new QueryFieldDTO()
                        {
                            FieldType = QueryFieldType.String,
                            Label = field.Name,
                            Name = field.Name,
                            Control = CreateTextBoxQueryControl(field.Name)
                        });
                    }
                }
            }
            return designTimeQueryFields;
        }
    }
}