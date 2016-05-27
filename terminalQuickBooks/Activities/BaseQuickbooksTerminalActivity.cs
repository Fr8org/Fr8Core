using System;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Fr8Data.Manifests;
using Newtonsoft.Json;
using terminalQuickBooks.Infrastructure;
using TerminalBase.BaseClasses;
using TerminalBase.Models;

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