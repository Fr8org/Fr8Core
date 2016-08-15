using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    /// <summary>
    ///  Not in service, but may provide useful ideas and insights
    /// </summary>
    public class Generate_DocuSign_Report_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("582A519E-7B1F-4424-B67B-EAA526C6953C"),
            Version = "1",
            Name = "Generate_DocuSign_Report",
            Label = "Generate DocuSign Report",
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        private const string QueryCrateLabel = "DocuSign Query";
        private const string SolutionName = "Generate DocuSign Report";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>This is Generate DocuSign Report solution action</p>";

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
            public readonly string FieldType;
            public readonly string DocusignQueryName;
            public readonly string MtDbPropertyName;
            public readonly Func<string, AuthorizationToken, ControlDefinitionDTO> ControlFactory;

            public FieldBackedRoutingInfo(
                string fieldType,
                string docusignQueryName,
                string mtDbPropertyName,
                Func<string, AuthorizationToken, ControlDefinitionDTO> controlFactory)
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
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }));

                Controls.Add(new Button
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

        private readonly IDocuSignManager _docuSignManager;
        private readonly PlanService _planService;

        // Mapping between quiery builder control field names and information about how this field is routed to the backed 
        private Dictionary<string, FieldBackedRoutingInfo> _queryBuilderFields;

        private static readonly string[] Statuses = {
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
        
        public Generate_DocuSign_Report_v1(ICrateManager crateManager, IDocuSignManager docuSignManager, PlanService planService)
            : base(crateManager)
        {
            _docuSignManager = docuSignManager;
            _planService = planService;
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

        public override async Task Run()
        {
            Success();
        }

        public override async Task RunChildActivities()
        {
            if (ConfigurationControls == null)
            {
                RaiseError("Action was not configured correctly");
            }

            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(ConfigurationControls);

            // Real-time search.
            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(actionUi.QueryBuilder.Value);
            var existingEnvelopes = new HashSet<string>();
            var searchResult = new StandardPayloadDataCM{ Name = "Docusign Report" };

            // Merge data from QueryMT action.
            var queryMTResult = Payload.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();

            MergeMtQuery(queryMTResult, existingEnvelopes, searchResult);

            // Update report crate.
            Payload.Add(Crate.FromContent("Sql Query Result", searchResult));

            RequestClientActivityExecution("ShowTableReport");

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
                    result.PayloadObject.Add(new KeyValueDTO(map.Item1, ""));
                }
                else
                {
                    string temp;
                    if (obj.TryGetValue(map.Item2, false, false, out temp))
                    {
                        result.PayloadObject.Add(new KeyValueDTO(map.Item1, temp ?? ""));
                    }
                    else
                    {
                        result.PayloadObject.Add(new KeyValueDTO(map.Item1, ""));
                    }
                }
            }

            return result;
        }
        
        public DocuSignQuery BuildDocusignQuery(DocuSignAuthTokenDTO authToken, List<FilterConditionDTO> conditions)
        {
            var query = new DocuSignQuery();
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

        public override async Task Initialize()
        {
            AddControls(new ActivityUi().Controls);
            Storage.AddRange(PackDesignTimeData());
            var plan = await _planService.UpdatePlanCategory(ActivityId, "report");
        }

        public override async Task FollowUp()
        {
            var activityTemplates = (await HubCommunicator.GetActivityTemplates(null, true))
                .Select(Mapper.Map<ActivityTemplateDO>)
                .ToList();

            try
            {
                var continueClicked = false;
                Storage.Remove<StandardQueryCM>();
                await UpdatePlanName();
                var queryCrate = ExtractQueryCrate(Storage);
                Storage.Add(queryCrate);

                var continueButton = GetControl<Button>("Continue");
                if (continueButton != null)
                {
                    continueClicked = continueButton.Clicked;

                    if (continueButton.Clicked)
                    {
                        continueButton.Clicked = false;
                    }
                }

                if (continueClicked)
                {
                    ActivityPayload.ChildrenActivities.Clear();

                    var queryFr8WarehouseActivityTemplate = activityTemplates
                        .FirstOrDefault(x => x.Name == "Query_Fr8_Warehouse");
                    if (queryFr8WarehouseActivityTemplate == null) { return; }

                    var queryFr8WarehouseTemplate = await HubCommunicator.GetActivityTemplate("terminalFr8Core", "Query_Fr8_Warehouse");

                    var queryFr8WarehouseAction = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, queryFr8WarehouseTemplate);

                    var crateStorage = queryFr8WarehouseAction.CrateStorage;
                       
                        var upstreamManifestTypes = crateStorage
                            .CrateContentsOfType<KeyValueListCM>(x => x.Label == "Upstream Crate ManifestType List")
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
                                upstreamCrateChooser.SelectedCrates[0].ManifestType.selectedKey = upstreamManifestTypes.Values[0].Key;
                                upstreamCrateChooser.SelectedCrates[0].ManifestType.Value = upstreamManifestTypes.Values[0].Value;
                            }

                            upstreamCrateChooser.SelectedCrates[0].Label.selectedKey = QueryCrateLabel;
                            upstreamCrateChooser.SelectedCrates[0].Label.Value = QueryCrateLabel;
                        }

                    queryFr8WarehouseAction = await HubCommunicator.ConfigureChildActivity(
                        ActivityPayload,
                        queryFr8WarehouseAction
                    );

                    Storage.RemoveByManifestId((int)MT.OperationalStatus);
                        var operationalStatus = new OperationalStateCM();
                        operationalStatus.CurrentActivityResponse =
                            ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity);
                        operationalStatus.CurrentActivityResponse.Body = "RunImmediately";
                    var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                    Storage.Add(operationsCrate);

                }
            }
            catch (Exception)
            {
            }
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

        private async Task<PlanDTO> UpdatePlanName()
        {
            if (ConfigurationControls != null)
            {
                var actionUi = new ActivityUi();
                actionUi.ClonePropertiesFrom(ConfigurationControls);
                var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                    actionUi.QueryBuilder.Value
                    );

                if (criteria.Count > 0)
                {
                    return await _planService.UpdatePlanName(ActivityId, "Generate a DocuSign Report", FilterConditionHelper.ParseConditionToText(criteria));
                }
            }

            return null;
        }

        public FieldDTO[] GetFieldListForQueryBuilder()
        {
            return _queryBuilderFields
                .Select(x =>
                    new FieldDTO()
                    {
                        Name = x.Key,
                        Label = x.Key,
                        FieldType = x.Value.FieldType
                    }
                )
                .ToArray();
        }

        private static ControlDefinitionDTO CreateTextBoxQueryControl(
            string key, AuthorizationToken authToken)
        {
            return new TextBox()
            {
                Name = "QueryField_" + key
            };
        }

        public ControlDefinitionDTO CreateFolderDropDownListControl(string key, AuthorizationToken authToken)
        {
            var conf = _docuSignManager.SetUp(AuthorizationToken);
            return new DropDownList()
            {
                Name = "QueryField_" + key,
                ListItems = DocuSignFolders.GetFolders(conf)
                    .Select(x => new ListItem() { Key = x.Key, Value = x.Value })
                    .ToList()
            };
        }

        private ControlDefinitionDTO CreateStatusDropDownListControl(
            string key, AuthorizationToken authToken)
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
            string key, AuthorizationToken authToken)
        {
            return new DatePicker()
            {
                Name = "QueryField_" + key
            };
        }

        private IEnumerable<Crate> PackDesignTimeData()
        {
            yield return Crate.FromContent(
                "Queryable Criteria",
                new FieldDescriptionsCM(GetFieldListForQueryBuilder())
            );

            yield return Crate.FromContent(
                "DocuSign Envelope Report",
                new KeyValueListCM(
                    new KeyValueDTO
                    {
                        Key = "DocuSign Envelope Report",
                        Value = "Table",
                    }
                )
            );
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
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainMailMerge"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution work with DocuSign Reports"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(new DocumentationResponseDTO("Unknown contentPath"));
            }
            return Task.FromResult(new DocumentationResponseDTO("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}