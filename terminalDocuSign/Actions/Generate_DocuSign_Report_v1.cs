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
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace terminalDocuSign.Actions
{
    public class Generate_DocuSign_Report_v1 : BaseTerminalActivity
    {
        private const string QueryCrateLabel = "DocuSign Query";


        // Here in this action we have query builder control to build queries against docusign API and out mt database.
        // Docusign and MT DB have different set of fileds and we want to provide ability to search by any field.
        // Our action should "plan" queries on the particular fields to the corresponding backend.
        // For example, we want to search by Status = Sent and Recipient = chucknorris@gmail.com
        // Both MT DB and Docusign can search by Status, but only MT DB can search by Recipient
        // We have to make two queries with the following criterias and union the results:
        // Docusign -> find all envelopes where Status = Sent
        // MT DB -> find all envelopes where Status = Sent and Recipient = chucknorris@gmail.com
        //
        // This little class is storing information about how certian field displayed in Query Builder controls is routed to the backed
        class FieldBackedRoutingInfo
        {
            public readonly string DocusignQueryName;
            public readonly string MtDbPropertyName;

            public FieldBackedRoutingInfo(string docusignQueryName, string mtDbPropertyName)
            {
                DocusignQueryName = docusignQueryName;
                MtDbPropertyName = mtDbPropertyName;
            }
        }

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
                    Value = "<p>Search for DocuSign Envelopes where the following are true:</p>"
                });

                var queryFields = GetFieldListForQueryBuilder();
                var filterConditions = new[]
                {
                    new FilterConditionDTO { Field = queryFields[0].Key, Operator = "eq" },
                    new FilterConditionDTO { Field = queryFields[1].Key, Operator = "eq" },
                    new FilterConditionDTO { Field = queryFields[2].Key, Operator = "eq" }
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

        // Mapping between quiery builder control field names and information about how this field is routed to the backed 
        private static readonly Dictionary<string, FieldBackedRoutingInfo> QueryBuilderFields = 
            new Dictionary<string, FieldBackedRoutingInfo>
            {
                { "Envelope Text", new FieldBackedRoutingInfo("SearchText", null) },
                { "Folder", new FieldBackedRoutingInfo("Folder", null) },
                { "Status", new FieldBackedRoutingInfo("Status", "Status") },
                { "CreateDate", new FieldBackedRoutingInfo("CreatedDateTime", "CreateDate") },
                { "SentDate", new FieldBackedRoutingInfo("SentDateTime", "SentDate") },
                // Did not find in FolderItem.
                // { "DeliveredDate", new FieldBackedRoutingInfo("DeliveredDate", "DeliveredDate") },
                // { "Recipient", new FieldBackedRoutingInfo("Recipient", "Recipient") },
                { "CompletedDate", new FieldBackedRoutingInfo("CompletedDateTime", "CompletedDate") },
                { "EnvelopeId", new FieldBackedRoutingInfo("EnvelopeId", "EnvelopeId") }
            };

        private readonly DocuSignManager _docuSignManager;
        private readonly IDocuSignFolder _docuSignFolder;

        public Generate_DocuSign_Report_v1()
        {
            _docuSignManager = ObjectFactory.GetInstance<DocuSignManager>();
            _docuSignFolder = ObjectFactory.GetInstance<IDocuSignFolder>();
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            CheckAuthentication(authTokenDO);

            return Success(payload);
        }

        public override async Task<PayloadDTO> ChildrenExecuted(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configurationControls = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var actionUi = new ActionUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            // Real-time search.
            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(actionUi.QueryBuilder.Value);
            var existingEnvelopes = new HashSet<string>();
            var searchResult = new StandardPayloadDataCM();
            var docuSignAuthToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            SearchDocusignInRealTime(docuSignAuthToken, criteria, searchResult, existingEnvelopes);

            // Merge data from QueryMT action.
            var payloadCrateStorage = Crate.FromDto(payload.CrateStorage);
            var queryMTResult = payloadCrateStorage
                .CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();

            MergeMtQuery(queryMTResult, existingEnvelopes, searchResult);

            // Update report crate.
            using (var updater = Crate.UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", searchResult));
            }

            return ExecuteClientAction(payload, "ShowTableReport");
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
                    searchResult.PayloadObjects.Add(queryMtObject);
                }
            }
        }

        private void SearchDocusignInRealTime(DocuSignAuthTokenDTO docuSignAuthToken, List<FilterConditionDTO> criteria, StandardPayloadDataCM searchResult, HashSet<string> existingEnvelopes)
        {
            var docusignQuery = BuildDocusignQuery(docuSignAuthToken, criteria);
            var envelopes = _docuSignManager.SearchDocusign(docuSignAuthToken, docusignQuery);

            foreach (var envelope in envelopes)
            {
                if (string.IsNullOrWhiteSpace(envelope.EnvelopeId))
                {
                    continue;
                }

                searchResult.PayloadObjects.Add(CreatePayloadObjectFromDocusignFolderItem(envelope));

                existingEnvelopes.Add(envelope.EnvelopeId);
            }
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

        private static PayloadObjectDTO CreatePayloadObjectFromEnvelope(DocuSignEnvelopeCM envelope)
        {
            var row = new PayloadObjectDTO();

            row.PayloadObject.Add(new FieldDTO("EnvelopeId", envelope.EnvelopeId));
            row.PayloadObject.Add(new FieldDTO("Name", string.Empty));
            row.PayloadObject.Add(new FieldDTO("Subject", string.Empty));
            row.PayloadObject.Add(new FieldDTO("Status", envelope.Status));
            row.PayloadObject.Add(new FieldDTO("OwnerName", string.Empty));
            row.PayloadObject.Add(new FieldDTO("SenderName", string.Empty));
            row.PayloadObject.Add(new FieldDTO("SenderEmail", string.Empty));
            row.PayloadObject.Add(new FieldDTO("Shared", string.Empty));
            row.PayloadObject.Add(new FieldDTO("CompletedDate", envelope.CompletedDate));
            row.PayloadObject.Add(new FieldDTO("CreatedDate", envelope.CreateDate));

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

                if (!QueryBuilderFields.TryGetValue(condition.Field, out fieldBackedRoutingInfo) || fieldBackedRoutingInfo.DocusignQueryName == null)
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
                            folders = _docuSignFolder.GetSearchFolders(authToken.Email, authToken.ApiPassword);
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

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.AddRange(PackDesignTimeData());
            }

            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            var activityTemplates = (await HubCommunicator.GetActivityTemplates(activityDO, null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();

            try
            {
                using (var updater = Crate.UpdateStorage(activityDO))
                {
                    updater.CrateStorage.Remove<StandardQueryCM>();

                    var queryCrate = ExtractQueryCrate(updater.CrateStorage);
                    updater.CrateStorage.Add(queryCrate);
                }

                activityDO.ChildNodes.Clear();

                var queryFr8WarehouseActivityTemplate = activityTemplates
                    .FirstOrDefault(x => x.Name == "QueryFr8Warehouse");
                if (queryFr8WarehouseActivityTemplate == null) { return activityDO; }

                var queryFr8WarehouseAction = await AddAndConfigureChildActivity(
                    activityDO,
                    "QueryFr8Warehouse"
                );

                using (var updater = Crate.UpdateStorage(queryFr8WarehouseAction))
                {
                    updater.CrateStorage.RemoveByLabel("Upstream Crate Label List");

                    var fields = new[]
                    {
                        new FieldDTO() { Key = QueryCrateLabel, Value = QueryCrateLabel }
                    };
                    var upstreamLabelsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Crate Label List", fields);
                    updater.CrateStorage.Add(upstreamLabelsCrate);

                    var upstreamManifestTypes = updater.CrateStorage
                        .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Crate ManifestType List")
                        .FirstOrDefault();

                    var controls = updater.CrateStorage
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

                // activityDO.ChildNodes.Add(new ActivityDO()
                // {
                //     ActivityTemplateId = queryFr8WarehouseAction.Id,
                //     IsTempId = true,
                //     Name = queryFr8WarehouseAction.Name,
                //     Label = queryFr8WarehouseAction.Label,
                //     CrateStorage = Crate.EmptyStorageAsStr(),
                //     ParentRouteNode = activityDO,
                //     Ordering = 1
                // });
            }
            catch (Exception)
            {
                return null;
            }


            return activityDO;
        }

        private Crate<StandardQueryCM> ExtractQueryCrate(CrateStorage storage)
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
                    Name = MT.DocuSignEnvelope.GetEnumDisplayName(),
                    Criteria = criteria
                }
            );

            return Crate<StandardQueryCM>.FromContent(QueryCrateLabel, queryCM);
        }

        public static FieldDTO[] GetFieldListForQueryBuilder()
        {
            return QueryBuilderFields.Keys.Select(x => new FieldDTO(x, x)).ToArray();
        }

        private IEnumerable<Crate> PackDesignTimeData()
        {
            yield return Data.Crates.Crate.FromContent("Queryable Criteria", new StandardDesignTimeFieldsCM(GetFieldListForQueryBuilder()));
            yield return Data.Crates.Crate.FromContent("DocuSign Envelope Report", new StandardDesignTimeFieldsCM(new FieldDTO
            {
                Key = "DocuSign Envelope Report",
                Value = "Table",
                Availability = AvailabilityType.RunTime
            }));
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}