using pluginSalesforce.Infrastructure;
using pluginSalesforce.sforce;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace pluginSalesforce.Services
{
    public class SalesforceAccount:ISalesforceAccount
    {
        private SforceService binding;
        public readonly string salesforceUserName;
        public readonly string salesforcePassword;

        public SalesforceAccount()
        {
            this.salesforceUserName = ConfigurationManager.AppSettings["SalesforceUserName"];
            this.salesforcePassword = ConfigurationManager.AppSettings["SalesforcePassword"];
            this.binding = new SforceService();

            LoginResult lr = binding.login(salesforceUserName, salesforcePassword);
            this.binding.Url = lr.serverUrl;
            this.binding.SessionHeaderValue = new SessionHeader();
            this.binding.SessionHeaderValue.sessionId = lr.sessionId;
        }

        public SforceService GetSalesForceServiceBinding()
        {
            return this.binding;
        }

    }
}