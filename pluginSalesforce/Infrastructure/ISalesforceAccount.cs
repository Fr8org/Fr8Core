using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using pluginSalesforce.sforce;

namespace pluginSalesforce.Infrastructure
{
    public interface ISalesforceAccount
    {
        SforceService GetSalesForceServiceBinding();
    }
}