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

namespace terminalDocuSign.Actions
{
    public class Generate_DocuSign_Report_v1 : BaseTerminalAction
    {
        private class FieldSetter
        {
            public readonly string Type;
            public readonly string Name;

            private FieldSetter(string type, string name)
            {
                Type = type;
                Name = name;
            }

            public static IEnumerable<FieldSetter> Parse(string setter)
            {
                var fields = setter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var field in fields)
                {
                    var info = field.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (info.Length != 2)
                    {
                        continue;
                    }

                    yield return new FieldSetter(info[0].Trim(), info[1].Trim());
                }
            }

            public void SetValue(object instance, string value)
            {
                if (instance == null) throw new ArgumentNullException("instance");

                var type = instance.GetType();
                var field = type.GetField(Name);
                object convertedValue;

                if (field != null && TryConvert (field.FieldType, value, out convertedValue))
                {
                    field.SetValue(instance, convertedValue);
                    return;
                }

                var prop = type.GetProperty(Name);
                if (prop != null && prop.CanWrite && TryConvert(prop.PropertyType, value, out convertedValue))
                {
                    prop.SetValue(instance, convertedValue);
                }
            }

            public bool TryConvert(Type targetType, string value, out object convertedValue)
            {
                if (targetType == typeof (string))
                {
                    convertedValue = value;
                    return true;
                }

                try
                {
                    convertedValue = Convert.ChangeType(value, targetType);
                    return true;
                }
                catch (Exception)
                {
                    convertedValue = null;
                    return false;
                }
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
                
                var queryFields = GetQueryFields();
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

        private readonly DocuSignManager _docuSignManager;
        private readonly IDocuSignFolder _docuSignFolder;
        
        public Generate_DocuSign_Report_v1()
        {
            _docuSignManager = ObjectFactory.GetInstance<DocuSignManager>();
            _docuSignFolder = ObjectFactory.GetInstance<IDocuSignFolder>();
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetProcessPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payload);
            }

            var ui = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (ui == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var config = new ActionUi();

            config.ClonePropertiesFrom(ui);

            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(config.QueryBuilder.Value);
            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
            var docusignQuery = CriteriaToDocusignQuery(docuSignAuthDto, criteria);
            var envelopes = _docuSignManager.SearchDocusign(docuSignAuthDto, docusignQuery);
            var existingEnvelopes = new HashSet<string>();
            var searchResult = new StandardPayloadDataCM();

            foreach (var envelope in envelopes)
            {
                if (string.IsNullOrWhiteSpace(envelope.EnvelopeId))
                {
                    continue;
                }

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
                
                searchResult.PayloadObjects.Add(row);
                existingEnvelopes.Add(envelope.EnvelopeId);
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var mtQuery = CriteriaToMtQuery(criteria, uow.MultiTenantObjectRepository.AsQueryable<DocuSignEnvelopeCM>(uow, authTokenDO.UserID));
                
                foreach (var envelope in mtQuery)
                {
                    if (!existingEnvelopes.Contains(envelope.EnvelopeId))
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

                        searchResult.PayloadObjects.Add(row);
                    }
                }
            }
            
            using (var updater = Crate.UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Report", searchResult));
            }

            return Success(payload);
        }

        public static IMtQueryable<DocuSignEnvelopeCM> CriteriaToMtQuery(List<FilterConditionDTO> conditions, IMtQueryable<DocuSignEnvelopeCM> queryable)
        {
            var type = typeof (DocuSignEnvelopeCM);
            var fields = GetQueryFields();

            ParameterExpression param = Expression.Parameter(type, "x");

            foreach (var condition in conditions)
            {
                var fieldName = condition.Field;
                var queryField = fields.FirstOrDefault(x => x.Key == fieldName);

                if (queryField == null)
                {
                    continue;
                }

                foreach (var setter in FieldSetter.Parse(queryField.Value).Where(x => x.Type == "mt"))
                {
                    Expression queryExpression = null;
                    var field = type.GetField(setter.Name);
                    object convertedValue;
                    Expression accessor;
                    

                    if (field != null && setter.TryConvert(field.FieldType, condition.Value, out convertedValue))
                    {
                        accessor = Expression.MakeMemberAccess(param, field);
                    }
                    else
                    {
                        var prop = type.GetProperty(setter.Name);
                        if (prop != null && prop.CanWrite && setter.TryConvert(prop.PropertyType, condition.Value, out convertedValue))
                        {
                            accessor = Expression.MakeMemberAccess(param, prop);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var operand = Expression.Constant(convertedValue);
                    
                    switch (condition.Operator)
                    {
                        case "eq":
                            queryExpression = Expression.Equal(accessor, operand);
                            break;

                        case "neq":
                            queryExpression = Expression.NotEqual(accessor, operand);
                            break;

                        case "gt":
                            queryExpression = Expression.GreaterThan(accessor, operand);
                            break;

                        case "gte":
                            queryExpression = Expression.GreaterThanOrEqual(accessor, operand);
                            break;

                        case "lte":
                            queryExpression = Expression.LessThanOrEqual(accessor, operand);
                            break;

                        case "lt":
                            queryExpression = Expression.LessThan(accessor, operand);
                            break;
                    }

                    if (queryExpression == null)
                    {
                        continue;
                    }

                    queryable = queryable.Where(Expression.Lambda<Func<DocuSignEnvelopeCM, bool>>(queryExpression, param));
                }
            }

            return queryable;
        }

        public DocusignQuery CriteriaToDocusignQuery(DocuSignAuth auth, List<FilterConditionDTO> conditions)
        {
            var query = new DocusignQuery();
            var fields = GetQueryFields();
            List<DocusignFolderInfo> folders = null;

            foreach (var condition in conditions.Where(x => x.Operator == "eq"))
            {
                var fieldName = condition.Field;
                var queryField = fields.FirstOrDefault(x => x.Key == fieldName);

                if (queryField == null)
                {
                    continue;
                }

                foreach (var setter in FieldSetter.Parse(queryField.Value).Where(x => x.Type == "docusign"))
                {
                    // criteria contains folder name, but to search we need folder id
                    if (setter.Name == "Folder")
                    {
                        if (folders == null)
                        {
                             folders = _docuSignFolder.GetFolders(auth.Email, auth.ApiPassword);
                        }

                        var value = condition.Value;
                        var folder = folders.FirstOrDefault(x => x.Name == value);

                        query.Folder = folder != null ? folder.FolderId : value;
                    }
                    else
                    {
                        setter.SetValue(query, condition.Value);
                    }
                }
            }

            return query;
        }
        
        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.AddRange(PackDesignTimeData());
            }
            
            return Task.FromResult(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return curActionDO;
        }

        public static FieldDTO[] GetQueryFields()
        {
            return new [] 
            {
                new FieldDTO("Envelope text", "docusign#SearchText"), 
                new FieldDTO("Folder", "docusign#Folder"), 
                new FieldDTO("Status", "docusign#Status, mt#Status"), 
            };
        }

        private IEnumerable<Crate> PackDesignTimeData()
        {
            yield return Data.Crates.Crate.FromContent("Queryable Criteria", new StandardDesignTimeFieldsCM(GetQueryFields()));
            yield return Data.Crates.Crate.FromContent("DocuSign Envelope Report", new StandardDesignTimeFieldsCM(new FieldDTO
            {
                Key = "DocuSign Envelope Report",
                Value = "Table",
                Availability = AvailabilityType.RunTime
            }));
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