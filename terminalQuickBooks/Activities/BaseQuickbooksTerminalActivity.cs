using System;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using terminalQuickBooks.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalQuickBooks.Actions
{
    public abstract class BaseQuickbooksTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        public BaseQuickbooksTerminalActivity() : base(true)
        {
        }

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return ex is TerminalQuickbooksTokenExpiredException;
        }
    }
}