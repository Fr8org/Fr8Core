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

        bool CreateContact(ActionDO actionDTO);

        bool CreateAccount(ActionDO actionDTO);
    }
}