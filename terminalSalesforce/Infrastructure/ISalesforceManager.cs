using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Interfaces.Manifests;
using Salesforce.Force;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceManager
    {
        ForceClient CreateForceClient(AuthorizationTokenDO authTokenDO);

        Task<bool> CreateObject<T>(T salesforceObject, string salesforceObjectType, ForceClient forceClient);

        Task<IList<FieldDTO>> GetFields(string salesforceObjectName, ForceClient forceClient);

        Task<StandardPayloadDataCM> GetObjectByQuery(string salesforceObjectName, string conditionQuery, ForceClient forceClient);
    }
}