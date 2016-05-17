using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Newtonsoft.Json;
using StructureMap;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Logging;

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
            if (result == false)
            {
                // Tries to refresh token. If refresh is successful, updates current token silently
                try
                {
                    var newToken = _googleIntegration.RefreshToken(token);
                    var tokenDTO = new AuthorizationTokenDTO()
                    {
                        Id = authTokenDO.Id.ToString(),
                        ExternalAccountId = authTokenDO.ExternalAccountId,
                        Token = JsonConvert.SerializeObject(newToken)
                    };
                    authTokenDO.Token = tokenDTO.Token;
                    HubCommunicator.RenewToken(tokenDTO, CurrentFr8UserId);
                    return false;
                }
                catch (Exception exception)
                {
                    var message = "Token is invalid and refresh failed with exception: " + exception.Message;
                    EventManager.TokenValidationFailed(authTokenDO.Token, message);
                    Logger.LogError(message);
                    return true;
                }
            }
            return false;
        }
    }
}