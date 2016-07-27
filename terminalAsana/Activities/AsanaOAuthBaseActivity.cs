using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Interfaces;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public abstract class AsanaOAuthBaseActivity<T> : TerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected AsanaClient AClient;

        public AsanaOAuthBaseActivity(ICrateManager crateManager, IAsanaParameters parameters, IRestfulServiceClient client) : base(crateManager)
        {
            // :( i want dependancy injection here ↓ 
            AClient = new AsanaClient(parameters, client);
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();
                   
            AClient.OAuth.RefreshTokenEvent += RefreshHubToken;

            var tokenData = JObject.Parse(this.AuthorizationToken.Token);
            var token = new OAuthToken
            {
                AccessToken = tokenData.Value<string>("access_token"),
                RefreshToken = tokenData.Value<string>("refresh_token"),
                ExpirationDate = this.AuthorizationToken.ExpiresAt ?? DateTime.MinValue
            };
            // ↓ not good :( , maybe change this method signature to Task? 
            AClient.OAuth = Task.Run(() => AClient.OAuth.InitializeAsync(token)).Result;   
        }

        /// <summary>
        /// method for event callback 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void RefreshHubToken(object sender, AsanaRefreshTokenEventArgs eventArgs)
        {
            var originalTokenData = JObject.Parse(this.AuthorizationToken.Token);
            originalTokenData["access_token"] = eventArgs.RefreshedToken.AccessToken;

            this.AuthorizationToken.AdditionalAttributes = eventArgs.RefreshedToken.ExpirationDate.ToString("O");
            this.AuthorizationToken.ExpiresAt = eventArgs.RefreshedToken.ExpirationDate;
            this.AuthorizationToken.Token = originalTokenData.ToString();
            try
            {
                var authDTO = Mapper.Map<AuthorizationTokenDTO>(this.AuthorizationToken);
                Task.Run(() => HubCommunicator.RenewToken(authDTO));
            }
            catch (Exception exp)
            {
                Logger.GetLogger("Asana terminal").Error($"terminalAsana: Error while token renew:  {exp.Message}");
            }
        }
    }
}