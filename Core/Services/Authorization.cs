using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Data.States;
using Newtonsoft.Json;
using Data.Interfaces.DataTransferObjects;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.Interfaces;

namespace Core.Services
{
    public class Authorization
    {
        public string GetToken(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindOne(at => at.UserID == userId);
                if (curAuthToken != null)
                    return curAuthToken.Token;
            }
            return null;
        }

        public string GetToken(string userId, int pluginId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindOne(at =>
                    at.UserID == userId
                    && at.PluginID == pluginId
                    && at.AuthorizationTokenState == AuthorizationTokenState.Active);

                if (curAuthToken != null)
                    return curAuthToken.Token;
            }
            return null;
        }

        public string GetPluginToken(int pluginId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindOne(at =>
                    at.PluginID == pluginId
                    && at.AuthorizationTokenState == AuthorizationTokenState.Active);

                if (curAuthToken != null)
                    return curAuthToken.Token;
            }
            return null;
        }


        public void AddOrUpdateToken(string userId, string token)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tokenDO = uow.AuthorizationTokenRepository.FindOne(at => at.UserID == userId);
                if (tokenDO == null)
                {
                    tokenDO = new AuthorizationTokenDO()
                    {
                        UserID = userId
                    };
                    uow.AuthorizationTokenRepository.Add(tokenDO);
                }
                tokenDO.ExpiresAt = DateTime.Now.AddYears(100);
                tokenDO.Token = token;
                uow.SaveChanges();
            }
        }

        public void RemoveToken(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tokenDO = uow.AuthorizationTokenRepository.FindOne(at => at.UserID == userId);
                if (tokenDO != null)
                {
                    uow.AuthorizationTokenRepository.Remove(tokenDO);
                    uow.SaveChanges();
                }
            }
        }


        /// <summary>
        /// Prepare AuthToken for ActionDTO request message.
        /// </summary>
        public void PrepareAuthToken(ActionDTO actionDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // Fetch ActivityTemplate.
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(actionDTO.ActivityTemplateId);
                if (activityTemplate == null)
                {
                    throw new ApplicationException("Could not find ActivityTemplate.");
                }

                // Fetch Action.
                var action = uow.ActionRepository.GetByKey(actionDTO.Id);
                if (action == null)
                {
                    throw new ApplicationException("Could not find Action.");
                }

                // Try to find AuthToken if plugin requires authentication.
                if (activityTemplate.Plugin.RequiresAuthentication)
                {
                    // Try to get owner's account for Action -> Route.
                    // Can't follow guideline to init services inside constructor. 
                    // Current implementation of Route and Action services are not good and are depedant on each other.
                    // Initialization of services in constructor will cause stack overflow
                    var route = ObjectFactory.GetInstance<IRoute>().GetRoute(action);
                    var dockyardAccount = route != null ? route.Fr8Account : null;

                    if (dockyardAccount == null)
                    {
                        throw new ApplicationException("Could not find DockyardAccount for Action's Route.");
                    }

                    var accountId = dockyardAccount.Id;

                    // Try to find AuthToken for specified plugin and account.
                    var authToken = uow.AuthorizationTokenRepository
                        .FindOne(x => x.Plugin.Id == activityTemplate.Plugin.Id
                            && x.UserDO.Id == accountId);

                    // If AuthToken is not empty, fill AuthToken property for ActionDTO.
                    if (authToken != null && !string.IsNullOrEmpty(authToken.Token))
                    {
                        actionDTO.AuthToken = new AuthTokenDTO()
                        {
                            Token = authToken.Token,
                            AdditionalAttributes = authToken.AdditionalAttributes
                        };
                    }
                }
            }
        }

        public async Task AuthenticateInternal(Fr8AccountDO account, PluginDO plugin,
         string username, string password)
        {
            if (!plugin.RequiresAuthentication)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new CredentialsDTO()
            {
                Username = username,
                Password = password
            };

            var response = await restClient.PostAsync<CredentialsDTO>(
                new Uri("http://" + plugin.Endpoint + "/actions/authenticate_internal"),
                credentialsDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthTokenDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(x => x.UserDO.Id == account.Id && x.Plugin.Id == plugin.Id);

                if (authTokenDTO != null)
                {
                    var curPlugin = uow.PluginRepository.GetByKey(plugin.Id);
                    var curAccount = uow.UserRepository.GetByKey(account.Id);

                    if (authToken == null)
                    {
                        authToken = new AuthorizationTokenDO()
                        {
                            Token = authTokenDTO.Token,
                            ExternalAccountId = authTokenDTO.ExternalAccountId,
                            Plugin = curPlugin,
                            UserDO = curAccount,
                            ExpiresAt = DateTime.Today.AddMonths(1)
                        };

                        uow.AuthorizationTokenRepository.Add(authToken);
                    }
                    else
                    {
                        authToken.Token = authTokenDTO.Token;
                        authToken.ExternalAccountId = authTokenDTO.ExternalAccountId;
                    }

                    uow.SaveChanges();
                }
            }
        }

        public async Task AuthenticateExternal(
            PluginDO plugin,
            ExternalAuthenticationDTO externalAuthDTO)
        {
            if (!plugin.RequiresAuthentication)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync<ExternalAuthenticationDTO>(
                new Uri("http://" + plugin.Endpoint + "/actions/authenticate_external"),
                externalAuthDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthTokenDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(x => x.ExternalStateToken == authTokenDTO.ExternalStateToken);

                if (authToken == null)
                {
                    throw new ApplicationException("No AuthToken found with specified ExternalStateToken.");
                }

                authToken.Token = authTokenDTO.Token;
                authToken.ExternalAccountId = authTokenDTO.ExternalAccountId;
                authToken.ExternalStateToken = null;
                authToken.AdditionalAttributes = authTokenDTO.AdditionalAttributes;
                uow.SaveChanges();
            }
        }


        public async Task<ExternalAuthUrlDTO> GetExternalAuthUrl(
            Fr8AccountDO user, PluginDO plugin)
        {
            if (!plugin.RequiresAuthentication)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync(
                new Uri("http://" + plugin.Endpoint + "/actions/auth_url")
            );

            var externalAuthUrlDTO = JsonConvert.DeserializeObject<ExternalAuthUrlDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(x => x.Plugin.Id == plugin.Id
                        && x.UserDO.Id == user.Id);

                if (authToken == null)
                {
                    var curPlugin = uow.PluginRepository.GetByKey(plugin.Id);
                    var curAccount = uow.UserRepository.GetByKey(user.Id);

                    authToken = new AuthorizationTokenDO()
                    {
                        UserDO = curAccount,
                        Plugin = curPlugin,
                        ExpiresAt = DateTime.Today.AddMonths(1),
                        ExternalStateToken = externalAuthUrlDTO.ExternalStateToken
                    };

                    uow.AuthorizationTokenRepository.Add(authToken);
                }
                else
                {
                    authToken.ExternalAccountId = null;
                    authToken.Token = null;
                    authToken.ExternalStateToken = externalAuthUrlDTO.ExternalStateToken;
                }

                uow.SaveChanges();
            }

            return externalAuthUrlDTO;
        }

    }
}
