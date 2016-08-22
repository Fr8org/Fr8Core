using System;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using terminalQuickBooks.Infrastructure;

namespace terminalQuickBooks.Actions
{
    public abstract class BaseQuickbooksTerminalActivity<T> : TerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseQuickbooksTerminalActivity(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return ex is TerminalQuickbooksTokenExpiredException;
        }
    }
}