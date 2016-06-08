using System;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using terminalSalesforce.Services;
using TerminalBase.BaseClasses;

namespace terminalSalesforce.Actions
{
    public abstract class BaseSalesforceTerminalActivity<T> : EnhancedTerminalActivity<T>
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