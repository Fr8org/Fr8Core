using System;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using terminalQuickBooks.Infrastructure;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalQuickBooks.Actions
{
    public abstract class BaseQuickbooksTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseQuickbooksTerminalActivity(ICrateManager crateManager)
            : base(true, crateManager)
        {
        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return ex is TerminalQuickbooksTokenExpiredException;
        }
    }
}