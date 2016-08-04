using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceManager
    {
        Task<string> Create(SalesforceObjectType type, IDictionary<string, object> @object, AuthorizationToken authToken);

        Task<StandardTableDataCM> Query(SalesforceObjectType type, IList<FieldDTO> propertiesToRetrieve, string filter, AuthorizationToken authToken);
        
        Task<List<FieldDTO>> GetProperties(SalesforceObjectType type, AuthorizationToken authToken, bool updatableOnly = false, string label = null);

        Task<string> PostToChatter(string message, string parentObjectId, AuthorizationToken authToken);

        IEnumerable<FieldDTO> GetSalesforceObjectTypes(SalesforceObjectOperations filterByOperations = SalesforceObjectOperations.None, SalesforceObjectProperties filterByProperties = SalesforceObjectProperties.None);

        Task<bool> Delete(SalesforceObjectType objectType, string objectId, AuthorizationToken authToken);

        [Obsolete("Use Task<StandardTableDataCM> Query(SalesforceObjectType, IEnumerable<string>, string, AuthorizationTokenDO) instead")]
        Task<IList<KeyValueDTO>> GetUsersAndGroups(AuthorizationToken authToken);
    }
}