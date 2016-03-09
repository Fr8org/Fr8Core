using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Salesforce.Force;
using terminalSalesforce.Infrastructure;
using Data.Entities;
using Salesforce.Common.Models;
using Salesforce.Common;
using Salesforce.Chatter;
using Newtonsoft.Json.Linq;
using Salesforce.Chatter.Models;

namespace terminalSalesforce.Services
{
    public class SalesforceManager : ISalesforceManager
    {
        private Authentication _authentication = new Authentication();
        private SalesforceObjectFactory salesforceObjectFactory = new SalesforceObjectFactory();
        private SalesforceObject _salesforceObject;

        /// <summary>
        /// Creates Salesforce object
        /// </summary>
        public async Task<bool> CreateObject<T>(T newObject, string salesforceObjectName, AuthorizationTokenDO authTokenDO)
        {
            bool createFlag = true;

            _salesforceObject = GetSalesforceObject(salesforceObjectName);
            SuccessResponse createCallResponse = null;

            var forceClient = (ForceClient)CreateSalesforceClient(typeof(ForceClient), authTokenDO);

            try
            {
                createCallResponse = await _salesforceObject.Create(newObject, salesforceObjectName, forceClient);
            }
            catch (ForceException salesforceException)
            {
                if (salesforceException.Message.Equals("Session expired or invalid"))
                {
                    forceClient = (ForceClient)CreateSalesforceClient(typeof(ForceClient), authTokenDO, true);
                    createCallResponse = await _salesforceObject.Create(newObject, salesforceObjectName, forceClient);
                }
                else
                {
                    throw salesforceException;
                }
            }

            if (string.IsNullOrEmpty(createCallResponse.Id))
            {
                createFlag = false;
            }

            return createFlag;
        }

        /// <summary>
        /// Gets Fields of the given Salesforce Object Name
        /// </summary>
        public async Task<IList<FieldDTO>> GetFields(string salesforceObjectName, AuthorizationTokenDO authTokenDO)
        {
            _salesforceObject = GetSalesforceObject(salesforceObjectName);
            var forceClient = (ForceClient)CreateSalesforceClient(typeof(ForceClient), authTokenDO);

            IList<FieldDTO> objectFields = null;
            try
            {
                objectFields = await _salesforceObject.GetFields(salesforceObjectName, forceClient);
            }
            catch (ForceException salesforceException)
            {
                if (salesforceException.Message.Equals("Session expired or invalid"))
                {
                    forceClient = (ForceClient)CreateSalesforceClient(typeof(ForceClient), authTokenDO, true);
                    objectFields = await _salesforceObject.GetFields(salesforceObjectName, forceClient);
                }
                else
                {
                    throw salesforceException;
                }
            }

            return objectFields;
        }

        /// <summary>
        /// Gets Salesforce objects by given query. The query will be executed agains the given Salesforce Object Name
        /// </summary>
        public async Task<StandardPayloadDataCM> GetObjectByQuery(string salesforceObjectName, string conditionQuery, AuthorizationTokenDO authTokenDO)
        {
            _salesforceObject = GetSalesforceObject(salesforceObjectName);
            var forceClient = (ForceClient)CreateSalesforceClient(typeof(ForceClient), authTokenDO);

            IList<PayloadObjectDTO> resultObjects = null;
            try
            {
                resultObjects = await _salesforceObject.GetByQuery(conditionQuery, forceClient);
            }
            catch (ForceException salesforceException)
            {
                if (salesforceException.Message.Equals("Session expired or invalid"))
                {
                    forceClient = (ForceClient)CreateSalesforceClient(typeof(ForceClient), authTokenDO, true);
                    resultObjects = await _salesforceObject.GetByQuery(conditionQuery, forceClient);
                }
                else
                {
                    throw salesforceException;
                }
            }

            return new StandardPayloadDataCM
            {
                ObjectType = string.Format("Salesforce {0}s", salesforceObjectName),
                PayloadObjects = resultObjects.ToList()
            };
        }

        /// <summary>
        /// Gets all avaialble chatter persons and chatter objects in the form of FieldDTO
        /// FieldDTO's Key as ObjectName and Value as ObjectId
        /// </summary>
        public async Task<IList<FieldDTO>> GetChatters(AuthorizationTokenDO authTokenDO)
        {
            var chatterClient = (ChatterClient)CreateSalesforceClient(typeof(ChatterClient), authTokenDO);

            var chatterObjectSelectPredicate = new Dictionary<string, Func<JToken, FieldDTO>>();
            chatterObjectSelectPredicate.Add("groups", 
                group => new FieldDTO(group.Value<string>("name"), group.Value<string>("id"), Data.States.AvailabilityType.Configuration));
            chatterObjectSelectPredicate.Add("users", 
                user => new FieldDTO(user.Value<string>("displayName"), user.Value<string>("id"), Data.States.AvailabilityType.Configuration));

            try
            {
                return await GetChattersAsFieldDTOsList(chatterClient, chatterObjectSelectPredicate);
            }
            catch (ForceException salesforceException)
            {
                if (salesforceException.Message.Equals("Session expired or invalid"))
                {
                    chatterClient = (ChatterClient)CreateSalesforceClient(typeof(ChatterClient), authTokenDO, true);

                    return await GetChattersAsFieldDTOsList(chatterClient, chatterObjectSelectPredicate);
                }
                else
                {
                    throw salesforceException;
                }
            }
        }

        public async Task<bool> PostFeedTextToChatterObject(string feedText, string parentObjectId, AuthorizationTokenDO authTokenDO)
        {
            var chatterClient = (ChatterClient)CreateSalesforceClient(typeof(ChatterClient), authTokenDO);

            try
            {
                return await PostFeedText(feedText, parentObjectId,  chatterClient);
            }
            catch (ForceException salesforceException)
            {
                if (salesforceException.Message.Equals("Session expired or invalid"))
                {
                    chatterClient = (ChatterClient)CreateSalesforceClient(typeof(ChatterClient), authTokenDO, true);

                    return await PostFeedText(feedText, parentObjectId, chatterClient);
                }
                else
                {
                    throw salesforceException;
                }
            }
        }

        public T CreateSalesforceDTO<T>(ActivityDO curActivity, PayloadDTO curPayload,
                                        Func<ActivityDO, PayloadDTO, string, string> extractControlValue)
        {
            var requiredType = typeof (T);
            var requiredObject = (T)Activator.CreateInstance(requiredType);
            var requiredProperties = requiredType.GetProperties().Where(p => !p.Name.Equals("Id"));

            requiredProperties.ToList().ForEach(prop =>
            {
                try
                {
                    var propValue = extractControlValue(curActivity, curPayload, prop.Name);
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
                    else if(applicationException.Message.StartsWith("No field found with specified key:"))
                    {
                        //FR-2502 - This else case handles, the user asked to pick up the value from the current payload.
                        //But the payload does not contain the value of this property. In that case, set it as "Not Available"
                        prop.SetValue(requiredObject, "Not Available");
                    }
                }
            });

            return requiredObject;
        }

        private object CreateSalesforceClient(Type requiredClientType, AuthorizationTokenDO authTokenDO, bool isRefreshTokenRequired = false)
        {
            AuthorizationTokenDO authTokenResult = null;

            //refresh the token only when it is required for the user of this method
            if (isRefreshTokenRequired)
            {
                authTokenResult = Task.Run(() => _authentication.RefreshAccessToken(authTokenDO)).Result;
            }
            else
            {
                //else consider the supplied authtoken itself
                authTokenResult = authTokenDO;
            }

            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenResult.AdditionalAttributes, out instanceUrl, out apiVersion);

            if (requiredClientType == typeof(ForceClient))
            {
                return new ForceClient(instanceUrl, authTokenResult.Token, apiVersion);
            }

            if (requiredClientType == typeof(ChatterClient))
            {
                return new ChatterClient(instanceUrl, authTokenResult.Token, apiVersion);
            }

            throw new NotSupportedException("Passed type is not supported. Supported Salesforce client objects are ForceClient and ChatterClient");
        }

        /// <summary>
        /// Gets required type of SalesforceObject from the Factory for the given Salesforce Object Name
        /// </summary>
        private SalesforceObject GetSalesforceObject(string salesforceObjectName)
        {
            return salesforceObjectFactory.GetSalesforceObject(salesforceObjectName);
        }

        private void ParseAuthToken(string authonTokenAdditionalValues, out string instanceUrl, out string apiVersion)
        {
            int startIndexOfInstanceUrl = authonTokenAdditionalValues.IndexOf("instance_url");
            int startIndexOfApiVersion = authonTokenAdditionalValues.IndexOf("api_version");
            instanceUrl = authonTokenAdditionalValues.Substring(startIndexOfInstanceUrl, (startIndexOfApiVersion - 1 - startIndexOfInstanceUrl));
            apiVersion = authonTokenAdditionalValues.Substring(startIndexOfApiVersion, authonTokenAdditionalValues.Length - startIndexOfApiVersion);
            instanceUrl = instanceUrl.Replace("instance_url=", "");
            apiVersion = apiVersion.Replace("api_version=", "");
        }

        private async Task<IList<FieldDTO>> GetChattersAsFieldDTOsList(
                    ChatterClient chatterClient,
                    IDictionary<string, Func<JToken, FieldDTO>> chatterObjectSelectPredicateCollection)
        {
            var chatterNamesList = new List<FieldDTO>();

            //get chatter groups and persons
            var chatterObjects = (JObject) await chatterClient.GetGroupsAsync<object>();
            chatterObjects.Merge((JObject) await chatterClient.GetUsersAsync<object>());

            JToken requiredChatterObjects;
            chatterObjectSelectPredicateCollection.ToList().ForEach(selectPredicate =>
            {
                requiredChatterObjects = null;
                if (chatterObjects.TryGetValue(selectPredicate.Key, out requiredChatterObjects) && requiredChatterObjects is JArray)
                {
                    chatterNamesList.AddRange(requiredChatterObjects.Select(a => selectPredicate.Value(a)));
                }
            });

            return chatterNamesList;
        }

        private async Task<bool> PostFeedText(string feedText, string parentObjectId, ChatterClient chatterClient)
        {
            var currentChatterUser = await chatterClient.MeAsync<UserDetail>();

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

            var feedItem = await chatterClient.PostFeedItemAsync<FeedItem>(feedItemInput, currentChatterUser.id);

            if(feedItem != null && !string.IsNullOrEmpty(feedItem.Id))
            {
                return true;
            }

            return false;
        }
    }
}