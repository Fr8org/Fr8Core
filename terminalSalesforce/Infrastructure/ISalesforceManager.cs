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
        Task<bool> CreateObject<T>(T salesforceObject, string salesforceObjectType, AuthorizationTokenDO authTokenDO);

        Task<IList<FieldDTO>> GetFields(string salesforceObjectName, AuthorizationTokenDO authTokenDO);

        Task<StandardPayloadDataCM> GetObjectByQuery(string salesforceObjectName, string conditionQuery, AuthorizationTokenDO authTokenDO);

        T CreateSalesforceDTO<T>(ActivityDO curActivity, PayloadDTO curPayload, Func<ActivityDO, PayloadDTO, string, string> extractControlValue);
    }
}