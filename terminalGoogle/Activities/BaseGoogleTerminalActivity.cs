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

        public override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (base.NeedsAuthentication(authTokenDO))
            {
                return true;
            }
            var token = GetGoogleAuthToken(authTokenDO);
            // we may also post token to google api to check its validity
            return token.Expires - DateTime.Now < TimeSpan.FromMinutes(5) && string.IsNullOrEmpty(token.RefreshToken);
        }
    }
}