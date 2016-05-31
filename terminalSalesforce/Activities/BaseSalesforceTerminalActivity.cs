using System;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using terminalSalesforce.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalSalesforce.Actions
{
    public abstract class BaseSalesforceTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseSalesforceTerminalActivity(ICrateManager crateManager)
            : base(true, crateManager)
        {
        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return SalesforceAuthHelper.IsTokenInvalidation(ex);
        }
    }
}