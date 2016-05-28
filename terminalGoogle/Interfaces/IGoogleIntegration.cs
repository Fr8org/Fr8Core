using Google.GData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;

namespace terminalGoogle.Interfaces
{
    public interface IGoogleIntegration
    {
        OAuth2Parameters CreateOAuth2Parameters(
           string accessCode = null,
           string accessToken = null,
           string refreshToken = null,
           string state = null);
        GOAuth2RequestFactory CreateRequestFactory(GoogleAuthDTO authDTO);
        string CreateOAuth2AuthorizationUrl(string state = null);
        GoogleAuthDTO GetToken(string code);
        GoogleAuthDTO RefreshToken(GoogleAuthDTO googleAuthDTO);
        Task<string> GetExternalUserId(GoogleAuthDTO googleAuthDTO);
        Task<bool> IsTokenInfoValid(GoogleAuthDTO googleAuthDTO);
    }
}