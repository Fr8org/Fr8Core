using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Salesforce.Force;
using terminalSalesforce.Infrastructure;
using Data.Entities;
using Hub.Managers;
using Salesforce.Common.Models;
using Salesforce.Common;
using Salesforce.Chatter;
using Newtonsoft.Json.Linq;
using Salesforce.Chatter.Models;
using StructureMap;
using Data.States;
using System.Linq.Expressions;

namespace terminalSalesforce.Services
{
    public class SalesforceManager : ISalesforceManager
    {
        private Authentication _authentication = new Authentication();

        private ICrateManager _crateManager;

        public SalesforceManager()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }
        
        public async Task<string> CreateObject(IDictionary<string, object> salesforceObject, string salesforceObjectName, AuthorizationTokenDO authTokenDO)
        {
            var result = await ExecuteClientOperationWithTokenRefresh(CreateForceClient, x => x.CreateAsync(salesforceObjectName, salesforceObject), authTokenDO);
            return result?.Id ?? string.Empty;
        }

        /// <summary>
        /// Gets Fields of the given Salesforce Object Name
        /// </summary>
        public async Task<IList<FieldDTO>> GetFields(string salesforceObjectName, AuthorizationTokenDO authTokenDO, bool onlyUpdatableFields = false)
        {
            var responce = (JObject)await ExecuteClientOperationWithTokenRefresh(CreateForceClient, x => x.DescribeAsync<object>(salesforceObjectName), authTokenDO);
            var objectFields = new List<FieldDTO>();
            JToken resultFields;
            if (responce.TryGetValue("fields", out resultFields) && resultFields is JArray)
            {
                if (onlyUpdatableFields)
                {
                    resultFields = new JArray(resultFields.Where(fieldDescription => (fieldDescription.Value<bool>("updateable") == true)));
                }

                var fields = resultFields.Select(fieldDescription =>
                                    /*
                                    Select Fields as FieldDTOs with                                    

                                    Key -> Field Name
                                    Value -> Field Lable
                                    AvailabilityType -> Run Time
                                    FieldType -> Field Type

                                    IsRequired -> The Field is required when ALL the below conditions are true.
                                      nillable            = false, Meaning, the field must have a valid value. The field's value should not be NULL or NILL or Empty
                                      defaultedOnCreate   = false, Meaning, Salesforce itself does not assign default value for this field when object is created (ex. ID)
                                      updateable          = true,  Meaning, The filed's value must be updatable by the user. 
                                                                            User must be able to set or modify the value of this field.
                                    */
                                    new FieldDTO(fieldDescription.Value<string>("name"), fieldDescription.Value<string>("label"), Data.States.AvailabilityType.RunTime)
                                    {
                                        FieldType = fieldDescription.Value<string>("type"),

                                        IsRequired = fieldDescription.Value<bool>("nillable") == false &&
                                                     fieldDescription.Value<bool>("defaultedOnCreate") == false &&
                                                     fieldDescription.Value<bool>("updateable") == true,
                                        Availability = AvailabilityType.RunTime

                                    }).OrderBy(field => field.Key);

                objectFields.AddRange(fields);
            }
            return objectFields;
        }

        public async Task<StandardTableDataCM> QueryObjects(string salesforceObjectName, IEnumerable<string> fields, string conditionQuery, AuthorizationTokenDO authTokenDO)
        {
            var selectQuery = string.Format("select {0} from {1}", string.Join(", ", fields), salesforceObjectName);
            if (!string.IsNullOrEmpty(conditionQuery))
            {
                selectQuery += " where " + conditionQuery;
            }
            var result = await ExecuteClientOperationWithTokenRefresh(CreateForceClient, x => x.QueryAsync<object>(selectQuery), authTokenDO);
            var table = ParseQueryResult(result);
            table.FirstRowHeaders = true;
            table.Table.Insert(0, new TableRowDTO { Row = fields.Select(x => new FieldDTO { Key = x, Value = x }).Select(x => new TableCellDTO { Cell = x }).ToList() });
            return table;
        }

        /// <summary>
        /// Gets all avaialble chatter persons and chatter objects in the form of FieldDTO
        /// FieldDTO's Key as ObjectName and Value as ObjectId
        /// </summary>
        public async Task<IList<FieldDTO>> GetUsersAndGroups(AuthorizationTokenDO authTokenDO)
        {
            var chatterObjectSelectPredicate = new Dictionary<string, Func<JToken, FieldDTO>>();
            chatterObjectSelectPredicate.Add("groups", x => new FieldDTO(x.Value<string>("name"), x.Value<string>("id"), AvailabilityType.Configuration));
            chatterObjectSelectPredicate.Add("users", x => new FieldDTO(x.Value<string>("displayName"), x.Value<string>("id"), AvailabilityType.Configuration));
            var chatterNamesList = new List<FieldDTO>();
            //get chatter groups and persons
            var chatterObjects = (JObject)await ExecuteClientOperationWithTokenRefresh(CreateChatterClient, x => x.GetGroupsAsync<object>(), authTokenDO);
            chatterObjects.Merge((JObject)await ExecuteClientOperationWithTokenRefresh(CreateChatterClient, x => x.GetUsersAsync<object>(), authTokenDO));
            foreach (var predicatePair in chatterObjectSelectPredicate)
            {
                JToken requiredChatterObjects;
                if (chatterObjects.TryGetValue(predicatePair.Key, out requiredChatterObjects) && requiredChatterObjects is JArray)
                {
                    chatterNamesList.AddRange(requiredChatterObjects.Select(a => predicatePair.Value(a)));
                }
            }
            return chatterNamesList;
        }

        public async Task<string> PostFeedTextToChatterObject(string feedText, string parentObjectId, AuthorizationTokenDO authTokenDO)
        {
            var currentChatterUser = await ExecuteClientOperationWithTokenRefresh(CreateChatterClient, x => x.MeAsync<UserDetail>(), authTokenDO);
            //set the message segment with the given feed text
            var messageSegment = new MessageSegmentInput
            {
                Text = feedText,
                Type = "Text"
            };
            //prepare the body
            var body = new MessageBodyInput { MessageSegments = new List<MessageSegmentInput> { messageSegment } };
            //prepare feed item input by setting the given parent object id
            var feedItemInput = new FeedItemInput()
            {
                Attachment = null,
                Body = body,
                SubjectId = parentObjectId,
                FeedElementType = "FeedItem"
            };
            var feedItem = await ExecuteClientOperationWithTokenRefresh(CreateChatterClient, x => x.PostFeedItemAsync<FeedItem>(feedItemInput, currentChatterUser.id), authTokenDO);
            return feedItem?.Id ?? string.Empty;
        }

        public T CreateSalesforceDTO<T>(ActivityDO curActivity, PayloadDTO curPayload) where T : new()
        {
            var requiredType = typeof(T);
            var requiredObject = (T)Activator.CreateInstance(requiredType);
            var requiredProperties = requiredType.GetProperties().Where(p => !p.Name.Equals("Id"));

            var designTimeCrateStorage = _crateManager.GetStorage(curActivity.CrateStorage);
            var runTimeCrateStorage = _crateManager.FromDto(curPayload.CrateStorage);
            var controls = designTimeCrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controls == null)
            {
                throw new InvalidOperationException("Failed to find configuration controls crate");
            }

            requiredProperties.ToList().ForEach(prop =>
            {
                try
                {
                    var textSourceControl = controls.Controls.SingleOrDefault(c => c.Name == prop.Name) as TextSource;

                    if (textSourceControl == null)
                    {
                        throw new InvalidOperationException($"Unable to find TextSource control with name '{prop.Name}'");
                    }

                    var propValue = textSourceControl.GetValue(runTimeCrateStorage);
                    prop.SetValue(requiredObject, propValue);
                }
                catch (ApplicationException applicationException)
                {
                    //If it can not extract the property, user did not enter any value for this property.
                    //No problems. We can treat that value as empty and continue.
                    if (applicationException.Message.Equals("Could not extract recipient, unknown recipient mode."))
                    {
                        prop.SetValue(requiredObject, string.Empty);
                    }
                    else if (applicationException.Message.StartsWith("No field found with specified key:"))
                    {
                        //FR-2502 - This else case handles, the user asked to pick up the value from the current payload.
                        //But the payload does not contain the value of this property. In that case, set it as "Not Available"
                        prop.SetValue(requiredObject, "Not Available");
                    }
                }
            });

            return requiredObject;
        }

        public IEnumerable<FieldDTO> GetObjectDescriptions()
        {
            return objectDescriptions ?? (objectDescriptions = new FieldDTO[]
                                                {
                                                    new FieldDTO("Account", "Account", AvailabilityType.Configuration),
                                                    new FieldDTO("Case", "Case", AvailabilityType.Configuration),
                                                    new FieldDTO("Contact", "Contact", AvailabilityType.Configuration),
                                                    new FieldDTO("Contract", "Contract", AvailabilityType.Configuration),
                                                    new FieldDTO("Document", "Document", AvailabilityType.Configuration),
                                                    new FieldDTO("Lead", "Lead", AvailabilityType.Configuration),
                                                    new FieldDTO("Opportunity", "Opportunity", AvailabilityType.Configuration),
                                                    new FieldDTO("Order", "Order", AvailabilityType.Configuration),
                                                    new FieldDTO("Product2", "Product2", AvailabilityType.Configuration),
                                                    new FieldDTO("Solution", "Solution", AvailabilityType.Configuration),
                                                });
        }

        public async Task<bool> DeleteObject(string salesforceObjectName, string objectId, AuthorizationTokenDO authTokenDO)
        {
            return await ExecuteClientOperationWithTokenRefresh(CreateForceClient, x => x.DeleteAsync(salesforceObjectName, objectId), authTokenDO);
        }

        #region Implemetation details
        private StandardTableDataCM ParseQueryResult(QueryResult<object> queryResult)
        {
            var parsedObjects = new List<JObject>();
            if (queryResult.Records.Count > 0)
            {
                parsedObjects = queryResult.Records.Select(record => ((JObject)record)).ToList();
            }
            return new StandardTableDataCM
            {
                Table = parsedObjects.Select(x => x.Properties()
                                            .Where(y => y.Value.Type == JTokenType.String && !string.IsNullOrEmpty(y.Value.Value<string>()))
                                            .Select(y => new FieldDTO
                                            {
                                                Key = y.Name,
                                                Value = y.Value.Value<string>()
                                            })
                                            .Select(y => new TableCellDTO { Cell = y }))
                                     .Select(x => new TableRowDTO { Row = x.ToList() })
                                     .ToList()
            };
        }

        private async Task<TResult> ExecuteClientOperationWithTokenRefresh<TClient, TResult>(Func<AuthorizationTokenDO, bool, Task<TClient>> clientProvider,
                                                                                             Func<TClient, Task<TResult>> operation,
                                                                                             AuthorizationTokenDO authTokenDO)
        {
            var client = await clientProvider(authTokenDO, false);
            var retried = false;
            Execution:
            try
            {
                return await operation(client);
            }
            catch (ForceException ex)
            {
                if (ex.Message.Equals("Session expired or invalid") && !retried)
                {
                    retried = true;
                    client = await clientProvider(authTokenDO, true);
                    goto Execution;
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<ForceClient> CreateForceClient(AuthorizationTokenDO authTokenDO, bool isRefreshTokenRequired = false)
        {
            authTokenDO = isRefreshTokenRequired ? await _authentication.RefreshAccessToken(authTokenDO) : authTokenDO;
            var salesforceToken = ToSalesforceToken(authTokenDO);
            return new ForceClient(salesforceToken.InstanceUrl, salesforceToken.Token, salesforceToken.ApiVersion);
        }

        private async Task<ChatterClient> CreateChatterClient(AuthorizationTokenDO authTokenDO, bool isRefreshTokenRequired = false)
        {
            authTokenDO = isRefreshTokenRequired ? await _authentication.RefreshAccessToken(authTokenDO) : authTokenDO;
            var salesforceToken = ToSalesforceToken(authTokenDO);
            return new ChatterClient(salesforceToken.InstanceUrl, salesforceToken.Token, salesforceToken.ApiVersion);
        }

        private IEnumerable<FieldDTO> objectDescriptions;

        private SalesforceAuthToken ToSalesforceToken(AuthorizationTokenDO ourToken)
        {
            var startIndexOfInstanceUrl = ourToken.AdditionalAttributes.IndexOf("instance_url");
            var startIndexOfApiVersion = ourToken.AdditionalAttributes.IndexOf("api_version");
            var instanceUrl = ourToken.AdditionalAttributes.Substring(startIndexOfInstanceUrl, (startIndexOfApiVersion - 1 - startIndexOfInstanceUrl));
            var apiVersion = ourToken.AdditionalAttributes.Substring(startIndexOfApiVersion, ourToken.AdditionalAttributes.Length - startIndexOfApiVersion);
            instanceUrl = instanceUrl.Replace("instance_url=", "");
            apiVersion = apiVersion.Replace("api_version=", "");
            return new SalesforceAuthToken { ApiVersion = apiVersion, InstanceUrl = instanceUrl, Token = ourToken.Token };
        }

        private struct SalesforceAuthToken
        {
            public string Token { get; set; }

            public string InstanceUrl { get; set; }

            public string ApiVersion { get; set; }
        }

        #endregion
    }
}