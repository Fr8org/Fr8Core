using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalStatX.Interfaces;

namespace terminalStatX.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IStatXIntegration _statXIntegration;
        private readonly IHubLoggerService _hubLoggerService;

        public AuthenticationController(IHubLoggerService hubLoggerService, IStatXIntegration statXIntegration)
        {
            _hubLoggerService = hubLoggerService;
            _statXIntegration = statXIntegration;
        }

        [HttpPost]
        [Route("send_code")]
        public async Task<PhoneNumberCredentialsDTO> SendAuthenticationCodeToMobilePhone(PhoneNumberCredentialsDTO credentialsDTO)
        {
            try
            {
                var statXAuthResponse = await _statXIntegration.Login(credentialsDTO.ClientName, credentialsDTO.PhoneNumber);

                if (!string.IsNullOrEmpty(statXAuthResponse.Error))
                {
                    credentialsDTO.Error = statXAuthResponse.Error;
                }
               
                credentialsDTO.ClientId = statXAuthResponse.ClientId;
                credentialsDTO.Title = "Enter the verification code from your StatX App: ";
                credentialsDTO.Message = "* To find your verification code, go to your StatX App (download if necessary from an App store), then tap \"Settings\", \"Additional Authorizations\", and finally \"Get Code\". Note: the code is NOT in your SMS messages.";

                return credentialsDTO;
            }
            catch (Exception ex)
            {
                await _hubLoggerService.ReportTerminalError(ex, credentialsDTO.Fr8UserId);
                credentialsDTO.Error = "An error occurred while trying to send login code, please try again later.";

                return credentialsDTO;
            }
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateAccessTokenAndApiKey(PhoneNumberCredentialsDTO credentialsDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(credentialsDTO.ClientId) || string.IsNullOrEmpty(credentialsDTO.PhoneNumber) || string.IsNullOrEmpty(credentialsDTO.VerificationCode))
                {
                    throw new ApplicationException("Provided data about verification code or phone number are missing.");
                }

                var authResponseDTO = await _statXIntegration.VerifyCodeAndGetAuthToken(credentialsDTO.ClientId, credentialsDTO.PhoneNumber, credentialsDTO.VerificationCode);

                return new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(authResponseDTO),
                    ExternalAccountId = credentialsDTO.ClientName,
                };
            }
            catch (Exception ex)
            {
                await _hubLoggerService.ReportTerminalError(ex, credentialsDTO.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }
    }
}