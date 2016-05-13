using System;
using Fr8Data.Manifests;
using terminalSalesforce.Services;
using TerminalBase.BaseClasses;

namespace terminalSalesforce.Actions
{
    public abstract class BaseSalesforceTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseSalesforceTerminalActivity() : base(true)
        {
        }

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return SalesforceAuthHelper.IsTokenInvalidation(ex);
        }
    }
}