using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using Newtonsoft.Json;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace Hub.Services
{
    public class Authorization
    {
        private readonly ICrateManager _crate;


        public Authorization()
        {
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

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
                if (activityTemplate.AuthenticationType != AuthenticationType.None)
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

        public async Task<string> AuthenticateInternal(
            Fr8AccountDO account,
            ActivityTemplateDO activityTemplate,
            string username,
            string password)
        {
            if (activityTemplate.AuthenticationType == AuthenticationType.None)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var plugin = activityTemplate.Plugin;

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new CredentialsDTO()
            {
                Username = username,
                Password = password
            };

            var response = await restClient.PostAsync<CredentialsDTO>(
                new Uri("http://" + plugin.Endpoint + "/authentication/internal"),
                credentialsDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthTokenDTO>(response);
            if (!string.IsNullOrEmpty(authTokenDTO.Error))
            {
                return authTokenDTO.Error;
            }

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

            return null;
        }

        public async Task<string> GetOAuthToken(
            PluginDO plugin,
            ExternalAuthenticationDTO externalAuthDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var hasAuthentication = uow.ActivityTemplateRepository
                    .GetQuery()
                    .Any(x => x.Plugin.Id == plugin.Id);

                if (!hasAuthentication)
                {
                    throw new ApplicationException("Plugin does not require authentication.");
                }
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync<ExternalAuthenticationDTO>(
                new Uri("http://" + plugin.Endpoint + "/authentication/token"),
                externalAuthDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthTokenDTO>(response);
            if (!string.IsNullOrEmpty(authTokenDTO.Error))
            {
                return authTokenDTO.Error;
            }

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

            return null;
        }


        public async Task<ExternalAuthUrlDTO> GetOAuthInitiationURL(
            Fr8AccountDO user,
            ActivityTemplateDO activityTemplate)
        {
            if (activityTemplate.AuthenticationType == AuthenticationType.None)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var plugin = activityTemplate.Plugin;

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync(
                new Uri("http://" + plugin.Endpoint + "/authentication/initial_url")
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

        private void AddAuthenticationCrate(
            ActionDTO actionDTO, int authType)
        {
            if (actionDTO.CrateStorage == null)
            {
                actionDTO.CrateStorage = new CrateStorageDTO()
                {
                    CrateDTO = new List<CrateDTO>()
                };
            }

            var mode = authType == AuthenticationType.Internal
                ? AuthenticationMode.InternalMode
                : AuthenticationMode.ExternalMode;

            actionDTO.CrateStorage.CrateDTO.Add(
                _crate.CreateAuthenticationCrate("RequiresAuthentication", mode)
            );
        }

        private void RemoveAuthenticationCrate(ActionDTO actionDTO)
        {
            if (actionDTO.CrateStorage != null
                && actionDTO.CrateStorage.CrateDTO != null)
            {
                var authCrates = actionDTO.CrateStorage.CrateDTO
                    .Where(x => x.ManifestType == CrateManifests.STANDARD_AUTHENTICATION_NAME)
                    .ToList();

                foreach (var authCrate in authCrates)
                {
                    actionDTO.CrateStorage.CrateDTO.Remove(authCrate);
                }
            }
        }

        public bool ValidateAuthenticationNeeded(string userId, ActionDTO curActionDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(curActionDTO.ActivityTemplateId);

                if (activityTemplate == null)
                {
                    throw new NullReferenceException("ActivityTemplate was not found.");
                }

                var account = uow.UserRepository.GetByKey(userId);

                if (account == null)
                {
                    throw new NullReferenceException("Current account was not found.");
                }

                if (activityTemplate.AuthenticationType != AuthenticationType.None)
                {
                    var authToken = uow.AuthorizationTokenRepository
                        .FindOne(x => x.Plugin.Id == activityTemplate.Plugin.Id
                            && x.UserDO.Id == account.Id);

                    if (authToken == null || string.IsNullOrEmpty(authToken.Token))
                    {
                        AddAuthenticationCrate(curActionDTO, activityTemplate.AuthenticationType);
                        return true;
                    }
                    else
                    {
                        RemoveAuthenticationCrate(curActionDTO);
                    }
                }
            }

            return false;
        }
    }
}
