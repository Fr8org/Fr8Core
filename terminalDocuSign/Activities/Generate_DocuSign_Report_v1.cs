using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalDocuSign.Activities
{
    public class Generate_DocuSign_Report_v1 : BaseTerminalActivity
    {
        private const string QueryCrateLabel = "DocuSign Query";
        private const string SolutionName = "Generate DocuSign Report";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>This is Generate DocuSign Report solution action</p>";

        private const int MaxResultSize = 1000;

        // Here in this action we have query builder control to build queries against docusign API and out mt database.
        // Docusign and MT DB have different set of fileds and we want to provide ability to search by any field.
        // Our action should "plan" queries on the particular fields to the corresponding backend.
        // For example, we want to search by Status = Sent and Recipient = chucknorris@gmail.com
        // Both MT DB and Docusign can search by Status, but only MT DB can search by Recipient
        // We have to make two queries with the following criterias and union the results:
        // Docusign -> find all envelopes where Status = Sent
        // MT DB -> find all envelopes where Status = Sent and Recipient = chucknorris@gmail.com
        //
        // This little class is storing information about how certian field displayed in Query Builder controls is query to the backed
        class FieldBackedRoutingInfo
        {
            public readonly FieldType FieldType;
            public readonly string DocusignQueryName;
            public readonly string MtDbPropertyName;
            public readonly Func<string, AuthorizationTokenDO, ControlDefinitionDTO> ControlFactory;

            public FieldBackedRoutingInfo(
                FieldType fieldType,
                string docusignQueryName,
                string mtDbPropertyName,
                Func<string, AuthorizationTokenDO, ControlDefinitionDTO> controlFactory)
            {
                FieldType = fieldType;
                DocusignQueryName = docusignQueryName;
                MtDbPropertyName = mtDbPropertyName;
                ControlFactory = controlFactory;
            }
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public QueryBuilder QueryBuilder { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for DocuSign Envelopes where:</p>"
                });

                var filterConditions = new[]
                {
                    new FilterConditionDTO { Field = "Envelope Text", Operator = "eq" },
                    new FilterConditionDTO { Field = "Folder", Operator = "eq" },
                    new FilterConditionDTO { Field = "Status", Operator = "eq" }
                };

                string initialQuery = JsonConvert.SerializeObject(filterConditions);

                Controls.Add((QueryBuilder = new QueryBuilder
                {
                    Name = "QueryBuilder",
                    Value = initialQuery,
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Criteria",
                        ManifestType = CrateManifestTypes.StandardQueryFields
                    }
                }));

                Controls.Add(new Button()
                {
                    Label = "Generate Report",
                    Name = "Continue",
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
                });
            }
        }

        private IDocuSignManager _docuSignManager;

        // Mapping between quiery builder control field names and information about how this field is routed to the backed 
        private Dictionary<string, FieldBackedRoutingInfo> _queryBuilderFields;

        private static readonly string[] Statuses = new[]
        {
            "Created",
            "Deleted",
            "Sent",
            "Delivered",
            "Signed",
            "Completed",
            "Declined",
            "Voided",
            "TimedOut",
            "AuthoritativeCopy",
            "TransferCompleted",
            "Template",
            "Correct"
        };
        
        private readonly IPlan _plan;
        public Generate_DocuSign_Report_v1()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
            _docuSignManager = ObjectFactory.GetInstance<IDocuSignManager>();
            InitQueryBuilderFields();
        }

        private void InitQueryBuilderFields()
        {
            _queryBuilderFields = new Dictionary<string, FieldBackedRoutingInfo>
            {
                {
                    "Envelope Text",
                    new FieldBackedRoutingInfo(FieldType.String, "SearchText", null, CreateTextBoxQueryControl)
                },
                {
                    "Folder",
                    new FieldBackedRoutingInfo(FieldType.String, "Folder", null, CreateFolderDropDownListControl)
                },
                {
                    "Status",
                    new FieldBackedRoutingInfo(FieldType.String, "Status", "Status", CreateStatusDropDownListControl)
                },
                {
                    "CreateDate",
                    new FieldBackedRoutingInfo(FieldType.Date, "CreatedDateTime", "CreateDate", CreateDatePickerQueryControl)
                },
                {
                    "SentDate",
                    new FieldBackedRoutingInfo(FieldType.Date, "SentDateTime", "SentDate", CreateDatePickerQueryControl)
                },
                {
                    "CompletedDate",
                    new FieldBackedRoutingInfo(FieldType.Date, "CompletedDateTime", "CompletedDate", CreateDatePickerQueryControl)
                },
                {
                    "EnvelopeId",
                    new FieldBackedRoutingInfo(FieldType.String, "EnvelopeId", "EnvelopeId", CreateTextBoxQueryControl)
                }
            };
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payload);
            }

            return Success(payload);
        }

        public override async Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configurationControls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            // Real-time search.
            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(actionUi.QueryBuilder.Value);
            var existingEnvelopes = new HashSet<string>();
            var searchResult = new StandardPayloadDataCM() { Name = "Docusign Report" };

            // Commented out by yakov.gnusin in scope of FR-2462.
            // var docuSignAuthToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            // SearchDocusignInRealTime(docuSignAuthToken, criteria, searchResult, existingEnvelopes);

            // Merge data from QueryMT action.
            var payloadCrateStorage = CrateManager.FromDto(payload.CrateStorage);
            var queryMTResult = payloadCrateStorage
                .CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();

            MergeMtQuery(queryMTResult, existingEnvelopes, searchResult);

            // Update report crate.
            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                crateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", searchResult));
            }

            return ExecuteClientActivity(payload, "ShowTableReport");
        }

        private static void MergeMtQuery(
            StandardPayloadDataCM queryMtResult,
            HashSet<string> existingEnvelopes,
            StandardPayloadDataCM searchResult)
        {
            if (queryMtResult == null)
            {
                return;
            }

            foreach (var queryMtObject in queryMtResult.PayloadObjects)
            {
                var id = queryMtObject.GetValue("EnvelopeId");
                if (!existingEnvelopes.Contains(id))
                {
                    searchResult.PayloadObjects.Add(ConvertQueryMtToRealTimeData(queryMtObject));
                }
            }
        }

        private static readonly List<Tuple<string, string>> RealTimeToMtMap =
            new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("EnvelopeId", "EnvelopeId"),
                new Tuple<string, string>("Name", null),
                new Tuple<string, string>("Subject", null),
                new Tuple<string, string>("Status", "Status"),
                new Tuple<string, string>("OwnerName", null),
                new Tuple<string, string>("SenderName", "ExternalAccountId"),
                new Tuple<string, string>("SenderEmail", null),
                new Tuple<string, string>("Shared", null),
                new Tuple<string, string>("CompletedDate", "CompletedDate"),
                new Tuple<string, string>("CreatedDate", "CreateDate")
            };

        private static PayloadObjectDTO ConvertQueryMtToRealTimeData(PayloadObjectDTO obj)
        {
            var result = new PayloadObjectDTO();
            foreach (var map in RealTimeToMtMap)
            {
                if (map.Item2 == null)
                {
                    result.PayloadObject.Add(new FieldDTO(map.Item1, ""));
                }
                else
                {
                    string temp;
                    if (obj.TryGetValue(map.Item2, false, false, out temp))
                    {
                        result.PayloadObject.Add(new FieldDTO(map.Item1, temp ?? ""));
                    }
                    else
                    {
                        result.PayloadObject.Add(new FieldDTO(map.Item1, ""));
                    }
                }
            }

            return result;
        }

        private void SearchDocusignInRealTime(DocuSignAuthTokenDTO docuSignAuthToken, List<FilterConditionDTO> criteria, StandardPayloadDataCM searchResult, HashSet<string> existingEnvelopes)
        {
            //var docusignQuery = BuildDocusignQuery(docuSignAuthToken, criteria);
           // var envelopes = _docuSignManager.SearchDocusign(docuSignAuthToken, docusignQuery);

            //foreach (var envelope in envelopes)
            //{
            //    if (string.IsNullOrWhiteSpace(envelope.EnvelopeId))
            //    {
            //        continue;
            //    }

            //    searchResult.PayloadObjects.Add(CreatePayloadObjectFromDocusignFolderItem(envelope));

            //    existingEnvelopes.Add(envelope.EnvelopeId);
            //}
        }

        // FolderItem is something that was put into the Docusing filder and it is not strictly envelope in terms of Docusign API. 
        // In current case we use it as envelope
        private static PayloadObjectDTO CreatePayloadObjectFromDocusignFolderItem(FolderItem envelope)
        {
            var row = new PayloadObjectDTO();

            row.PayloadObject.Add(new FieldDTO("EnvelopeId", envelope.EnvelopeId));
            row.PayloadObject.Add(new FieldDTO("Name", envelope.Name));
            row.PayloadObject.Add(new FieldDTO("Subject", envelope.Subject));
            row.PayloadObject.Add(new FieldDTO("Status", envelope.Status));
            row.PayloadObject.Add(new FieldDTO("OwnerName", envelope.OwnerName));
            row.PayloadObject.Add(new FieldDTO("SenderName", envelope.SenderName));
            row.PayloadObject.Add(new FieldDTO("SenderEmail", envelope.SenderEmail));
            row.PayloadObject.Add(new FieldDTO("Shared", envelope.Shared));
            row.PayloadObject.Add(new FieldDTO("CompletedDate", envelope.CompletedDateTime.ToString(CultureInfo.InvariantCulture)));
            row.PayloadObject.Add(new FieldDTO("CreatedDate", envelope.CreatedDateTime.ToString(CultureInfo.InvariantCulture)));

            return row;
        }
        
        public DocusignQuery BuildDocusignQuery(DocuSignAuthTokenDTO authToken, List<FilterConditionDTO> conditions)
        {
            var query = new DocusignQuery();
            List<DocusignFolderInfo> folders = null;

            //Currently we can support only equality operation
            foreach (var condition in conditions)
            {
                FieldBackedRoutingInfo fieldBackedRoutingInfo;

                if (!_queryBuilderFields.TryGetValue(condition.Field, out fieldBackedRoutingInfo) || fieldBackedRoutingInfo.DocusignQueryName == null)
                {
                    continue;
                }

                // criteria contains folder name, but to search we need folder id
                if (fieldBackedRoutingInfo.DocusignQueryName == "Folder")
                {
                    if (condition.Operator == "eq")
                    {
                        // cache list of folders
                        if (folders == null)
                        {
                            //folders = _docuSignFolder.GetSearchFolders(authToken.Email, authToken.ApiPassword);
                        }

                        var value = condition.Value;
                        var folder = folders.FirstOrDefault(x => x.Name == value);

                        query.Folder = folder != null ? folder.FolderId : value;
                    }
                }
                else if (fieldBackedRoutingInfo.DocusignQueryName == "Status" && condition.Operator == "eq")
                {
                    query.Status = condition.Value;
                }
                else if (fieldBackedRoutingInfo.DocusignQueryName == "SearchText")
                {
                    if (condition.Operator == "eq")
                    {
                        query.SearchText = condition.Value;
                    }
                }
                else if (fieldBackedRoutingInfo.DocusignQueryName == "CreatedDateTime")
                {
                    DateTime dt;
                    if (condition.Operator == "gt" || condition.Operator == "gte")
                    {
                        if (DateTime.TryParseExact(condition.Value, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                        {
                            query.FromDate = dt;
                        }
                    }
                    else if (condition.Operator == "lt")
                    {
                        if (DateTime.TryParseExact(condition.Value, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                        {
                            query.ToDate = dt.AddDays(-1);
                        }
                    }
                    else if (condition.Operator == "lte")
                    {
                        if (DateTime.TryParseExact(condition.Value, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                        {
                            query.ToDate = dt;
                        }
                    }
                }
                else
                {
                    query.Conditions.Add(new FilterConditionDTO()
                    {
                        Field = fieldBackedRoutingInfo.DocusignQueryName,
                        Operator = condition.Operator,
                        Value = condition.Value
                    });
                }
            }

            return query;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(
            ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.AddRange(PackDesignTimeData(authTokenDO));
            }

            PlanFullDTO plan = await UpdatePlanCategory(curActivityDO.Id, "report");

            return await Task.FromResult(curActivityDO);
        }

        //private int ExtractDocuSignResultSize(
        //    DocuSignAuthTokenDTO authToken,
        //    List<FilterConditionDTO> criteria)
        //{
        //    var docusignQuery = BuildDocusignQuery(authToken, criteria);
        //    var count = _docuSignManager.CountEnvelopes(authToken, docusignQuery);

        //    return count;
        //}

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            var activityTemplates = (await HubCommunicator.GetActivityTemplates(null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();

            try
            {

                var continueClicked = false;

                using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
                {
                    crateStorage.Remove<StandardQueryCM>();
                    RemoveControl(crateStorage, "CannotProceedMessage");

                    await UpdatePlanName(activityDO);

                    var queryCrate = ExtractQueryCrate(crateStorage);
                    crateStorage.Add(queryCrate);

                    var controls = crateStorage
                        .CrateContentsOfType<StandardConfigurationControlsCM>()
                        .FirstOrDefault();

                    var continueButton = controls.FindByName<Button>("Continue");
                    if (continueButton != null)
                    {
                        continueClicked = continueButton.Clicked;

                        if (continueButton.Clicked)
                        {
                            continueButton.Clicked = false;
                        }
                    }

                    // Commented out by yakov.gnusin in scope of FR-2462.
                    // if (continueClicked)
                    // {
                    //     var docuSignAuthToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
                    //     var criteria = queryCrate.Content.Queries.First().Criteria;
                    //     var resultSize = ExtractDocuSignResultSize(docuSignAuthToken, criteria);
                    // 
                    //     if (resultSize > MaxResultSize)
                    //     {
                    //         continueClicked = false;
                    //         InsertControlAfter(
                    //             crateStorage,
                    //             new TextBlock()
                    //             {
                    //                 Name = "CannotProceedMessage",
                    //                 Value = "Fr8 can not currently generate this report because the set size is too big.",
                    //                 CssClass = "well well-lg"
                    //             },
                    //             "QueryBuilder"
                    //         );
                    //     }
                    // }
                }

                if (continueClicked)
                {
                    activityDO.ChildNodes.Clear();

                    var queryFr8WarehouseActivityTemplate = activityTemplates
                        .FirstOrDefault(x => x.Name == "QueryFr8Warehouse");
                    if (queryFr8WarehouseActivityTemplate == null) { return activityDO; }

                    var queryFr8WarehouseTemplate = await GetActivityTemplate("terminalFr8Core", "QueryFr8Warehouse");

                    var queryFr8WarehouseAction = await AddAndConfigureChildActivity(
                        activityDO,queryFr8WarehouseTemplate
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
                            .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Crate ManifestType List")
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
                            if (upstreamManifestTypes != null)
                            {
                                upstreamCrateChooser.SelectedCrates[0].ManifestType.selectedKey = upstreamManifestTypes.Fields[0].Key;
                                upstreamCrateChooser.SelectedCrates[0].ManifestType.Value = upstreamManifestTypes.Fields[0].Value;
                            }

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
                            ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity);
                        operationalStatus.CurrentClientActivityName = "RunImmediately";

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

        private Crate<StandardQueryCM> ExtractQueryCrate(ICrateStorage storage)
        {
            var configurationControls = storage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .SingleOrDefault();

            if (configurationControls == null)
            {
                throw new ApplicationException("Action was not configured correctly");
            }

            var actionUi = new ActivityUi();
            actionUi.ClonePropertiesFrom(configurationControls);

            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                actionUi.QueryBuilder.Value
            );

            // This is weird to use query's name as the way to address MT type. 
            // MT type has unique ID that should be used for this reason. Query name is something that is displayed to user. It should not contain any internal data.
            var queryCM = new StandardQueryCM(
                new QueryDTO()
                {
                    Name = MT.DocuSignEnvelope.GetEnumDisplayName(),
                    Criteria = criteria
                }
            );

            return Crate<StandardQueryCM>.FromContent(QueryCrateLabel, queryCM);
        }

        private async Task<PlanFullDTO> UpdatePlanName(ActivityDO activityDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                var configurationControls = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .SingleOrDefault();

                if (configurationControls != null)
                {
                    var actionUi = new ActivityUi();
                    actionUi.ClonePropertiesFrom(configurationControls);

                    var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                        actionUi.QueryBuilder.Value
                    );

                    if (criteria.Count > 0)
                    {
                        return await UpdatePlanName(activityDO.Id, "Generate a DocuSign Report", ParseConditionToText(criteria));
                    }
                }
            }
            return null;
        }

        public TypedFieldDTO[] GetFieldListForQueryBuilder(AuthorizationTokenDO authToken)
        {
            return _queryBuilderFields
                .Select(x =>
                    new TypedFieldDTO(
                        x.Key,
                        x.Key,
                        x.Value.FieldType,
                        x.Value.ControlFactory(x.Key, authToken)
                    )
                )
                .ToArray();
        }

        private static ControlDefinitionDTO CreateTextBoxQueryControl(
            string key, AuthorizationTokenDO authToken)
        {
            return new TextBox()
            {
                Name = "QueryField_" + key
            };
        }

        public ControlDefinitionDTO CreateFolderDropDownListControl(
        string key, AuthorizationTokenDO authToken)
        {
            var conf = _docuSignManager.SetUp(authToken);
            return new DropDownList()
            {
                Name = "QueryField_" + key,
                ListItems = DocuSignFolders.GetFolders(conf)
                    .Select(x => new ListItem() { Key = x.Key, Value = x.Value })
                    .ToList()
            };
        }

        private ControlDefinitionDTO CreateStatusDropDownListControl(
            string key, AuthorizationTokenDO authToken)
        {
            return new DropDownList()
            {
                Name = "QueryField_" + key,
                ListItems = Statuses
                    .Select(x => new ListItem() { Key = x, Value = x })
                    .ToList()
            };
        }

        private ControlDefinitionDTO CreateDatePickerQueryControl(
            string key, AuthorizationTokenDO authToken)
        {
            return new DatePicker()
            {
                Name = "QueryField_" + key
            };
        }

        private IEnumerable<Crate> PackDesignTimeData(AuthorizationTokenDO authToken)
        {
            yield return Data.Crates.Crate.FromContent(
                "Queryable Criteria",
                new TypedFieldsCM(GetFieldListForQueryBuilder(authToken))
            );

            yield return Data.Crates.Crate.FromContent(
                "DocuSign Envelope Report",
                new FieldDescriptionsCM(
                    new FieldDTO
                    {
                        Key = "DocuSign Envelope Report",
                        Value = "Table",
                        Availability = AvailabilityType.RunTime
                    }
                )
            );
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            return ConfigurationRequestType.Followup;
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
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainMailMerge"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonse(@"This solution work with DocuSign Reports"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonse(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonse("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonse("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}