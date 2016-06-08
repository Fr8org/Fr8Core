using System;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using terminalQuickBooks.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalQuickBooks.Actions
{
    public abstract class BaseQuickbooksTerminalActivity<T> : EnhancedTerminalActivity<T>
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