using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceManager
    {
        Task<string> CreateObject(IDictionary<string, object> salesforceObject, string salesforceObjectName, AuthorizationTokenDO authTokenDO);

        Task<StandardTableDataCM> QueryObjects(string salesforceObjectName, IEnumerable<string> fields, string conditionQuery, AuthorizationTokenDO authTokenDO);
        
        Task<IList<FieldDTO>> GetFields(string salesforceObjectName, AuthorizationTokenDO authTokenDO, bool onlyUpdatableFields = false);

        T CreateSalesforceDTO<T>(ActivityDO curActivity, PayloadDTO curPayload) where T : new();

        Task<IList<FieldDTO>> GetUsersAndGroups(AuthorizationTokenDO authTokenDO);

        Task<string> PostFeedTextToChatterObject(string feedText, string parentObjectId, AuthorizationTokenDO authTokenDO);

        IEnumerable<FieldDTO> GetObjectDescriptions();

        Task<bool> DeleteObject(string salesforceObjectName, string objectId, AuthorizationTokenDO authTokenDO);
    }
}