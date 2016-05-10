using System;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Manifests;
using Newtonsoft.Json;
using StructureMap;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;

namespace terminalGoogle.Actions
{
    public abstract class BaseGoogleTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        private readonly IGoogleIntegration _googleIntegration;

        protected BaseGoogleTerminalActivity() : base(true)
        {
            _googleIntegration = ObjectFactory.GetInstance<IGoogleIntegration>();
        }

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return GoogleAuthHelper.IsTokenInvalidation(ex);
        }

        public GoogleAuthDTO GetGoogleAuthToken(AuthorizationTokenDO authTokenDO = null)
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>((authTokenDO ?? AuthorizationToken).Token);
        }

        /// <summary>
        /// Checks if google token is invalid.
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <returns></returns>
        public override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (base.NeedsAuthentication(authTokenDO))
            {
                return true;
            }
            var token = GetGoogleAuthToken(authTokenDO);

            // Post token to google api to check its validity
            // Variable needs for more readability.
            var result = Task.Run(async () => await _googleIntegration.IsTokenInfoValid(token)).Result;
            return !result;
        }
    }
}