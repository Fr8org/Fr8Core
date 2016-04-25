using System;
using Data.Entities;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;

namespace terminalGoogle.Activities
{
    public abstract class BaseGoogleTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseGoogleTerminalActivity() : base(true)
        {
        }

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return GoogleAuthHelper.IsTokenInvalidation(ex);
        }
        public GoogleAuthDTO GetGoogleAuthToken(AuthorizationTokenDO authTokenDO = null)
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>((authTokenDO ?? AuthorizationToken).Token);
        }
    }
}