using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Fr8Data.Manifests;
using Newtonsoft.Json;
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

        public AuthorizationTokenDO GetQuickbooksAuthToken()
        {
            return AuthorizationToken;
        }
    }
}