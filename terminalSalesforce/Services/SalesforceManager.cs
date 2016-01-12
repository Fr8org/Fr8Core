using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Salesforce.Force;
using terminalSalesforce.Infrastructure;
using Utilities.Logging;
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