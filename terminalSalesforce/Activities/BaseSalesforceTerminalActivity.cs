using System;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using terminalSalesforce.Services;

namespace terminalSalesforce.Actions
{
    public abstract class BaseSalesforceTerminalActivity<T> : TerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseSalesforceTerminalActivity(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return SalesforceAuthHelper.IsTokenInvalidation(ex);
        }
    }
}