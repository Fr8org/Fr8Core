using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Logging;
using log4net;
using Newtonsoft.Json;
using terminalTelegram.TelegramIntegration;

namespace terminalTelegram.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger("terminalTelegram");

        private readonly ITelegramIntegration _telegramIntegration;

        public AuthenticationController(ITelegramIntegration telegramIntegration)
        {
            _telegramIntegration = telegramIntegration;
        }

        [HttpPost]
        [Route("send_code")]
        public async Task<PhoneNumberCredentialsDTO> SendAuthenticationCodeToMobilePhone(PhoneNumberCredentialsDTO credentialsDTO)
        {
            try
            {
                return await CreateTelegramConnection(credentialsDTO);
            }
            catch (Exception ex)
            {
                try
                {
                    // Telegram SDK contains a bug, so somethimes we need to send two auth codes to phone number
                    if (ex.Message == "STORE_INVALID_OBJECT_TYPE")
                    {
                        return await CreateTelegramConnection(credentialsDTO);
                    }
                    throw;
                }
                catch (Exception exception)
                {
                    credentialsDTO.Error = "An error occurred while trying to send login code, please try again later.";
                    credentialsDTO.Message = exception.Message + Environment.NewLine + exception.StackTrace;
                    Logger.Error(exception.Message);
                    return credentialsDTO;
                }
            }
        }

        private async Task<PhoneNumberCredentialsDTO> CreateTelegramConnection(PhoneNumberCredentialsDTO credentialsDTO)
        {
            await _telegramIntegration.ConnectAsync();
            var hash = await _telegramIntegration.GetHashAsync(credentialsDTO.PhoneNumber);

            credentialsDTO.ClientId = hash;

            credentialsDTO.Message = "* Verification code has been sent to your Telegram mobile app.";

            return credentialsDTO;
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

                var authResponseDTO = await _telegramIntegration.MakeAuthAsync(
                    credentialsDTO.PhoneNumber,
                    credentialsDTO.ClientId,
                    credentialsDTO.VerificationCode);
                return new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(authResponseDTO),
                    ExternalAccountId = credentialsDTO.ClientName,
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                credentialsDTO.Message = ex.Message;
                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }
    }
}