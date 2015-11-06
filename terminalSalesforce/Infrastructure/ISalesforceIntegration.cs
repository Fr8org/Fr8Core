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
        bool CreateLead(ActionDTO actionDTO);

        bool CreateContact(ActionDTO actionDTO);

        bool CreateAccount(ActionDTO actionDTO);
    }
}