using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceManager
    {
        Task<string> Create(SalesforceObjectType type, IDictionary<string, object> @object, AuthorizationTokenDO authTokenDO);

        Task<StandardTableDataCM> Query(SalesforceObjectType type, IEnumerable<string> propertiesToRetrieve, string filter, AuthorizationTokenDO authTokenDO);
        
        Task<List<FieldDTO>> GetProperties(SalesforceObjectType type, AuthorizationTokenDO authTokenDO, bool updatableOnly = false);

        T CreateSalesforceDTO<T>(ActivityDO activity, PayloadDTO payload) where T : new();

        Task<string> PostToChatter(string message, string parentObjectId, AuthorizationTokenDO authTokenDO);

        IEnumerable<FieldDTO> GetSalesforceObjectTypes(SalesforceObjectOperations filterByOperations = SalesforceObjectOperations.None, SalesforceObjectProperties filterByProperties = SalesforceObjectProperties.None);

        Task<bool> Delete(SalesforceObjectType objectType, string objectId, AuthorizationTokenDO authTokenDO);

        [Obsolete("Use Task<StandardTableDataCM> Query(SalesforceObjectType, IEnumerable<string>, string, AuthorizationTokenDO) instead")]
        Task<IList<FieldDTO>> GetUsersAndGroups(AuthorizationTokenDO authTokenDO);
    }
}