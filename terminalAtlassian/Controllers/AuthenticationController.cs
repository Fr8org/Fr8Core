using System.Web.Http;
using Newtonsoft.Json;
using terminalAtlassian.Services;
using Fr8.Infrastructure.Data.DataTransferObjects;
using terminalAtlassian.Helpers;
using System.Threading.Tasks;
using terminalAtlassian.Interfaces;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IAtlassianService _atlassianService;

        public AuthenticationController(AtlassianService atlassianService)
        {
            _atlassianService = atlassianService;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateInternalOAuthToken(CredentialsDTO credentials)
        {
            credentials = credentials.EnforceDomainSchema();
            if (!await _atlassianService.CheckDomain(credentials.Domain))
            {
                return new AuthorizationTokenDTO()
                {
                    Error = "The form of the domain is generally [yourprojectname].atlassian.net"
                };
            }
            if (await _atlassianService.CheckAuthenticationAsync(credentials))
            {
                return new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(credentials),
                    ExternalAccountId = credentials.Username,
                    AuthCompletedNotificationRequired = true
                };
            }
            return new AuthorizationTokenDTO()
            {
                Error = "Unable to authenticate in Atlassian service, invalid domain, login name or password."
            };
        }
    }
}