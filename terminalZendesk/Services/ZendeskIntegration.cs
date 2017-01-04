using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json.Linq;
using StructureMap;
using terminalZendesk.Interfaces;
using ZendeskApi_v2;
using ZendeskApi_v2.Models.Tickets;

namespace terminalZendesk.Services
{
    public class ZendeskIntegration : IZendeskIntegration
    {
        private readonly IRestfulServiceClient _client;

        public ZendeskIntegration(IRestfulServiceClient client)
        {
            _client = client;
            
        }

        /// <summary>
        /// Build external Zendesk OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var template = CloudConfigurationManager.GetSetting("ZendeskOAuthUrl");
            var url = template.Replace("%STATE%", externalStateToken);

            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var parameters = new
            {
                grant_type = "authorization_code",
                code = code,
                client_id = CloudConfigurationManager.GetSetting("ZendeskId"),
                client_secret = CloudConfigurationManager.GetSetting("ZendeskSecret"),
                redirect_uri = CloudConfigurationManager.GetSetting("HubOAuthRedirectUri"),
                scope = "read"
            };
            
            var template = CloudConfigurationManager.GetSetting("ZendeskOAuthAccessUrl");
            var jsonObj = JObject.Parse(await _client.PostAsync(new Uri(template), parameters));
            return jsonObj.Value<string>("access_token");
        }

        public async Task<UserInfo> GetUserInfo(string oauthToken)
        {
            var zendeskApi = new ZendeskApi(CloudConfigurationManager.GetSetting("ZendeskSubDomain"), oauthToken);
            var curUser = await zendeskApi.Users.GetCurrentUserAsync();
            // note we should use current user's subdomain not the subdomain carved into config file
            // TODO update this
            return new UserInfo
            {
                UserId = curUser.User.Id?.ToString() ?? curUser.User.Email,
                UserName = curUser.User.Name,
                SubDomain = CloudConfigurationManager.GetSetting("ZendeskSubDomain")
            };
        }

        public async Task CreateTicket(string oauthToken, string subject, string comment, string reqEmail, string reqName)
        {
            var zendeskApi = new ZendeskApi(CloudConfigurationManager.GetSetting("ZendeskSubDomain"), oauthToken);
            var ticket = new Ticket
            {
                Subject = subject,
                Requester = new Requester
                {
                    Email = reqEmail,
                    Name = reqName
                },
                Comment = new Comment
                {
                    Body = comment
                }
            };
            await zendeskApi.Tickets.CreateTicketAsync(ticket);
        }

    }
}