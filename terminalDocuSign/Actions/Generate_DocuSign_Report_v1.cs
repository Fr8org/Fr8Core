using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.Repositories;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
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
        // Here in this action we have query builder control to build queries against docusign API and out mt database.
        // Docusign and MT DB have different set of fileds and we want to provide ability to search by any field.
        // Our action should "route" queries on the particular fields to the corresponding backend.
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
                    new FilterConditionDTO {Field = queryFields[0].Key, Operator = "eq"},
                    new FilterConditionDTO {Field = queryFields[1].Key, Operator = "eq"},
                    new FilterConditionDTO {Field = queryFields[2].Key, Operator = "eq"}
                };
                
                string initialQuery = JsonConvert.SerializeObject(filterConditions);
                
                Controls.Add((QueryBuilder = new QueryBuilder
                {
                    Name = "QueryBuilder",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Value = initialQuery,
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Criteria",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }));
            }
        }

        // Mapping between quiery builder control field names and information about how this field is routed to the backed 
        private static readonly Dictionary<string, FieldBackedRoutingInfo> QueryBuilderFields = new Dictionary<string, FieldBackedRoutingInfo>
        {
            {"Envelope Text", new FieldBackedRoutingInfo("SearchText", null)},
            {"Folder", new FieldBackedRoutingInfo("Folder", null)},
            {"Status", new FieldBackedRoutingInfo("Status", "Status")}
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

            var configurationControls = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var actionUi = new ActionUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(actionUi.QueryBuilder.Value);
            var existingEnvelopes = new HashSet<string>();
            var searchResult = new StandardPayloadDataCM();
            var docuSignAuthToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            SearchDocusignInRealTime(docuSignAuthToken, criteria, searchResult, existingEnvelopes);
            SearchMtDataBase(authTokenDO, criteria, existingEnvelopes, searchResult);
            
            using (var updater = Crate.UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Report", searchResult));
            }

            return Success(payload);
        }

        private static void SearchMtDataBase(AuthorizationTokenDO authTokenDO, List<FilterConditionDTO> criteria, HashSet<string> existingEnvelopes, StandardPayloadDataCM searchResult)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelopes = MTSearchHelper.CreateQueryProvider(typeof (DocuSignEnvelopeCM)).Query(uow, authTokenDO.UserID, criteria);
                
                foreach (DocuSignEnvelopeCM envelope in envelopes)
                {
                    if (!existingEnvelopes.Contains(envelope.EnvelopeId))
                    {
                        searchResult.PayloadObjects.Add(CreatePayloadObjectFromEnvelope(envelope));
                    }
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

            row.PayloadObject.Add(new FieldDTO("Id", envelope.EnvelopeId));
            row.PayloadObject.Add(new FieldDTO("Name", envelope.Name));
            row.PayloadObject.Add(new FieldDTO("Subject", envelope.Subject));
            row.PayloadObject.Add(new FieldDTO("Status", envelope.Status));
            row.PayloadObject.Add(new FieldDTO("OwnerName", envelope.OwnerName));
            row.PayloadObject.Add(new FieldDTO("SenderName", envelope.SenderName));
            row.PayloadObject.Add(new FieldDTO("SenderEmail", envelope.SenderEmail));
            row.PayloadObject.Add(new FieldDTO("Shared", envelope.Shared));
            row.PayloadObject.Add(new FieldDTO("CompletedDate", envelope.CompletedDateTime.ToString(CultureInfo.InvariantCulture)));
            row.PayloadObject.Add(new FieldDTO("CreatedDateTime", envelope.CreatedDateTime.ToString(CultureInfo.InvariantCulture)));

            return row;
        }

        private static PayloadObjectDTO CreatePayloadObjectFromEnvelope(DocuSignEnvelopeCM envelope)
        {
            var row = new PayloadObjectDTO();

            row.PayloadObject.Add(new FieldDTO("Id", envelope.EnvelopeId));
            row.PayloadObject.Add(new FieldDTO("Name", string.Empty));
            row.PayloadObject.Add(new FieldDTO("Subject", string.Empty));
            row.PayloadObject.Add(new FieldDTO("Status", envelope.Status));
            row.PayloadObject.Add(new FieldDTO("OwnerName", string.Empty));
            row.PayloadObject.Add(new FieldDTO("SenderName", string.Empty));
            row.PayloadObject.Add(new FieldDTO("SenderEmail", string.Empty));
            row.PayloadObject.Add(new FieldDTO("Shared", string.Empty));
            row.PayloadObject.Add(new FieldDTO("CompletedDate", envelope.CompletedDate));
            row.PayloadObject.Add(new FieldDTO("CreatedDateTime", envelope.CreateDate));

            return row;
        }
        
        public DocusignQuery BuildDocusignQuery(DocuSignAuthTokenDTO authToken, List<FilterConditionDTO> conditions)
        {
            var query = new DocusignQuery();
            List<DocusignFolderInfo> folders = null;

            //Currently we can support only equality operation
            foreach (var condition in conditions.Where(x => x.Operator == "eq"))
            {
                FieldBackedRoutingInfo fieldBackedRoutingInfo;
                
                if (!QueryBuilderFields.TryGetValue(condition.Field, out fieldBackedRoutingInfo) || fieldBackedRoutingInfo.DocusignQueryName == null)
                {
                    continue;
                }

                switch (fieldBackedRoutingInfo.DocusignQueryName)
                {
                    // criteria contains folder name, but to search we need folder id
                    case "Folder":
                        // cache list of folders
                        if (folders == null)
                        {
                             folders = _docuSignFolder.GetSearchFolders(authToken.Email, authToken.ApiPassword);
                        }

                        var value = condition.Value;
                        var folder = folders.FirstOrDefault(x => x.Name == value);

                        query.Folder = folder != null ? folder.FolderId : value;
                        break;

                    case "Status":
                        query.Status = condition.Value;
                        break;
                    
                    case "SearchText":
                        query.SearchText = condition.Value;
                        break;
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

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return curActivityDO;
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