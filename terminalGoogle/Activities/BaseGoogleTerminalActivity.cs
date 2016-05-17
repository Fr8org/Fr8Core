using System;
using System.Threading.Tasks;
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
                        Id = AuthorizationToken.Id.ToString(),
                        ExternalAccountId = AuthorizationToken.ExternalAccountId,
                        Token = JsonConvert.SerializeObject(newToken)
                    };
                    AuthorizationToken.Token = tokenDTO.Token;
                    HubCommunicator.RenewToken(tokenDTO, CurrentUserId);
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