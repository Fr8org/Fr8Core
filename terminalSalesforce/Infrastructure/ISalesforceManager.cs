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
        Task<string> CreateObject<T>(T salesforceObject, string salesforceObjectType, AuthorizationTokenDO authTokenDO);

        Task<bool> DeleteObject(string sfObjectName, string sfObjectId, AuthorizationTokenDO authTokenDO);

        Task<IList<FieldDTO>> GetFields(string salesforceObjectName, AuthorizationTokenDO authTokenDO, bool onlyUpdatableFields = false);

        Task<StandardPayloadDataCM> GetObjectByQuery(string salesforceObjectName, IEnumerable<string> fields, string conditionQuery, AuthorizationTokenDO authTokenDO);

        Task<IList<FieldDTO>> GetChatters(AuthorizationTokenDO authTokenDO);

        Task<string> PostFeedTextToChatterObject(string feedText, string parentObjectId, AuthorizationTokenDO authTokenDO);
    }
}