using System;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;

namespace terminalGoogle.Actions
{
    public abstract class BaseGoogleTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        private readonly IGoogleIntegration _googleIntegration;

        protected BaseGoogleTerminalActivity(ICrateManager crateManager, IGoogleIntegration googleIntegration)
            : base(crateManager)
        {
            _googleIntegration = googleIntegration;

        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return GoogleAuthHelper.IsTokenInvalidation(ex);
        }

        public GoogleAuthDTO GetGoogleAuthToken()
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>(AuthorizationToken.Token);
        }

        /// <summary>
        /// Checks if google token is invalid.
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <returns></returns>
        public override bool NeedsAuthentication()
        {
            if (base.NeedsAuthentication())
            {
                return true;
            }
            var token = GetGoogleAuthToken();
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
                        Id = AuthorizationToken.Id,
                        ExternalAccountId = AuthorizationToken.ExternalAccountId,
                        Token = JsonConvert.SerializeObject(newToken)
                    };
                    AuthorizationToken.Token = tokenDTO.Token;
                    HubCommunicator.RenewToken(tokenDTO);
                    return false;
                }
                catch (Exception exception)
                {
                    var message = "Token is invalid and refresh failed with exception: " + exception.Message;
                    //EventManager.TokenValidationFailed(authTokenDO.Token, message);
                    Logger.LogError(message);
                    return true;
                }
            }
            return false;
        }
    }
}