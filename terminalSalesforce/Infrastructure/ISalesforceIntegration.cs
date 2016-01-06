using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceIntegration
    {
        bool CreateLead(ActionDO actionDO, AuthorizationTokenDO authTokenDO);

        bool CreateContact(ActionDO actionDTO, AuthorizationTokenDO authTokenDO);

        bool CreateAccount(ActionDO actionDTO, AuthorizationTokenDO authTokenDO);

        Task<IList<FieldDTO>> GetFieldsList(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string salesforceObjectName);

        Task<object> GetObject(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string salesforceObjectName, string condition);
    }
}