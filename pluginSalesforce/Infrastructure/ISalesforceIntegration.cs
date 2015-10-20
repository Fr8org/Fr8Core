using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace pluginSalesforce.Infrastructure
{
    public interface ISalesforceIntegration
    {
        bool CreateLead(ActionDTO actionDTO);       
    }
}