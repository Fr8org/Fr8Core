using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Salesforce.Force;
using terminalSalesforce.Infrastructure;
using Data.Entities;

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
        public async Task<bool> CreateObject<T>(T newObject, string salesforceObjectName, ForceClient forceClient)
        {
            bool createFlag = true;

            _salesforceObject = GetSalesforceObject(salesforceObjectName);
            var createCallResponse = await _salesforceObject.Create(newObject, salesforceObjectName, forceClient);

            if (string.IsNullOrEmpty(createCallResponse.Id))
            {
                createFlag = false;
            }

            return createFlag;
        }

        /// <summary>
        /// Gets Fields of the given Salesforce Object Name
        /// </summary>
        public async Task<IList<FieldDTO>> GetFields(string salesforceObjectName, ForceClient forceClient)
        {
            _salesforceObject = GetSalesforceObject(salesforceObjectName);
            return await _salesforceObject.GetFields(salesforceObjectName, forceClient);
        }

        /// <summary>
        /// Gets Salesforce objects by given query. The query will be executed agains the given Salesforce Object Name
        /// </summary>
        public async Task<StandardPayloadDataCM> GetObjectByQuery(string salesforceObjectName, string conditionQuery,
                                                                  ForceClient forceClient)
        {
            _salesforceObject = GetSalesforceObject(salesforceObjectName);
            var resultObjects = await _salesforceObject.GetByQuery(conditionQuery, forceClient);

            return new StandardPayloadDataCM
            {
                ObjectType = string.Format("Salesforce {0}s", salesforceObjectName),
                PayloadObjects = resultObjects.ToList()
            };
        }

        public ForceClient CreateForceClient(AuthorizationTokenDO authTokenDO)
        {
            //TODO: Vas, Created task FR-2037
            var authTokenResult = Task.Run(() => _authentication.RefreshAccessToken(authTokenDO)).Result;
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenResult.AdditionalAttributes, out instanceUrl, out apiVersion);
            return new ForceClient(instanceUrl, authTokenResult.Token, apiVersion);
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
    }
}