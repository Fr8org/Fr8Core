using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Salesforce.Force;

namespace pluginSalesforce.Infrastructure
{
    public interface IConfiguration
    {
        ForceClient GetForceClient();
    }
}