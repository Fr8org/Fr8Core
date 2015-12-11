using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Infrastructure;
using StructureMap;
using Newtonsoft.Json;
using Data.Crates;
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
    public class Authorization : IAuthorization
    {
        private readonly ICrateManager _crate;
	    private readonly ITime _time;


        public Authorization()
        {
			_crate = ObjectFactory.GetInstance<ICrateManager>();
	        _time = ObjectFactory.GetInstance<ITime>();
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

        public string GetToken(string userId, int terminalId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindOne(at =>
                    at.UserID == userId
                    && at.TerminalID == terminalId
                    && at.AuthorizationTokenState == AuthorizationTokenState.Active);

                if (curAuthToken != null)
                    return curAuthToken.Token;
            }
            return null;
        }

        public string GetTerminalToken(int terminalId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindOne(at =>
                    at.TerminalID == terminalId
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

	            DateTime currentTime = _time.CurrentDateTime();

				tokenDO.ExpiresAt = currentTime.AddYears(100);
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

                // Try to find AuthToken if terminal requires authentication.
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

                    // Try to find AuthToken for specified terminal and account.
                    // var authToken = uow.AuthorizationTokenRepository
                    //     .FindOne(x => x.Terminal.Id == activityTemplate.Terminal.Id
                    //         && x.UserDO.Id == accountId);

                    var actionDO = uow.ActionRepository.GetByKey(actionDTO.Id);
                    if (actionDO == null)
                    {
                        throw new ApplicationException("Could not find ActionDO for Action's RouteNode.");
                    }

                    var authToken = actionDO.AuthorizationToken;

                    // If AuthToken is not empty, fill AuthToken property for ActionDTO.
                    if (authToken != null && !string.IsNullOrEmpty(authToken.Token))
                    {
                        actionDTO.AuthToken = new AuthorizationTokenDTO()
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
            ActionDO actionDO,
            string domain,
            string username,
            string password)
        {
            if (actionDO.ActivityTemplate.AuthenticationType == AuthenticationType.None)
            {
                throw new ApplicationException("Terminal does not require authentication.");
            }

            var terminal = actionDO.ActivityTemplate.Terminal;

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new CredentialsDTO()
            {
                Domain = domain,
                Username = username,
                Password = password
            };

            var terminalResponse = await restClient.PostAsync<CredentialsDTO>(
                new Uri("http://" + terminal.Endpoint + "/authentication/internal"),
                credentialsDTO
            );

            var terminalResponseAuthTokenDTO = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(terminalResponse);
            if (!string.IsNullOrEmpty(terminalResponseAuthTokenDTO.Error))
            {
                return terminalResponseAuthTokenDTO.Error;
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionDO = uow.ActionRepository.GetByKey(actionDO.Id);
                if (curActionDO == null)
                {
                    throw new ApplicationException("Could not find ActionDO by Id specified.");
                }

                var authToken = curActionDO.AuthorizationToken;

                if (terminalResponseAuthTokenDTO != null)
                {
                    var curTerminal = uow.TerminalRepository.GetByKey(terminal.Id);
                    var curAccount = uow.UserRepository.GetByKey(account.Id);

                    if (authToken == null)
                    {
                        authToken = new AuthorizationTokenDO()
                        {
                            Token = terminalResponseAuthTokenDTO.Token,
                            ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId,
                            Terminal = curTerminal,
                            UserDO = curAccount,
                            ExpiresAt = DateTime.Today.AddMonths(1)
                        };

                        uow.AuthorizationTokenRepository.Add(authToken);
                    }
                    else
                    {
                        authToken.Token = terminalResponseAuthTokenDTO.Token;
                        authToken.ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId;
                    }

                    curActionDO.AuthorizationToken = authToken;

                    uow.SaveChanges();

                    //if terminal requires Authentication Completed Notification, follow the existing terminal event notification protocol 
                    //to notify the terminal about authentication completed event
                    if (terminalResponseAuthTokenDTO.AuthCompletedNotificationRequired)
                    {
                        EventManager.TerminalAuthenticationCompleted(curAccount.Id, curTerminal);
                    }
                }
            }

            return null;
        }

        public async Task<string> GetOAuthToken(
            TerminalDO terminal,
            ExternalAuthenticationDTO externalAuthDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var hasAuthentication = uow.ActivityTemplateRepository
                    .GetQuery()
                    .Any(x => x.Terminal.Id == terminal.Id);

                if (!hasAuthentication)
                {
                    throw new ApplicationException("Terminal does not require authentication.");
                }
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync<ExternalAuthenticationDTO>(
                new Uri("http://" + terminal.Endpoint + "/authentication/token"),
                externalAuthDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(response);
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
            ActionDO actionDO)
        {
            if (actionDO.ActivityTemplate.AuthenticationType == AuthenticationType.None)
            {
                throw new ApplicationException("Terminal does not require authentication.");
            }

            var terminal = actionDO.ActivityTemplate.Terminal;

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync(
                new Uri("http://" + terminal.Endpoint + "/authentication/initial_url")
            );

            var externalAuthUrlDTO = JsonConvert.DeserializeObject<ExternalAuthUrlDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionDO = uow.ActionRepository.GetByKey(actionDO.Id);
                if (curActionDO == null)
                {
                    throw new ApplicationException("Could not find ActionDO by Id specified.");
                }

                var authToken = curActionDO.AuthorizationToken;

                if (authToken == null)
                {
                    var curTerminal = uow.TerminalRepository.GetByKey(terminal.Id);
                    var curAccount = uow.UserRepository.GetByKey(user.Id);

                    authToken = new AuthorizationTokenDO()
                    {
                        UserDO = curAccount,
                        Terminal = curTerminal,
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

                curActionDO.AuthorizationToken = authToken;

                uow.SaveChanges();
            }

            return externalAuthUrlDTO;
        }

        public void AddAuthenticationCrate(ActionDTO actionDTO, int authType)
        {
            using (var updater = _crate.UpdateStorage(() => actionDTO.CrateStorage))
            {
                AuthenticationMode mode = authType == AuthenticationType.Internal ? AuthenticationMode.InternalMode : AuthenticationMode.ExternalMode;

                switch (authType)
                {
                    case AuthenticationType.Internal:
                        mode = AuthenticationMode.InternalMode;
                        break;
                    case AuthenticationType.External:
                        mode = AuthenticationMode.ExternalMode;
                        break;
                    case AuthenticationType.InternalWithDomain:
                        mode = AuthenticationMode.InternalModeWithDomain;
                        break;
                    case AuthenticationType.None:
                    default:
                        mode = AuthenticationMode.ExternalMode;
                        break;
                }

                updater.CrateStorage.Add(_crate.CreateAuthenticationCrate("RequiresAuthentication", mode));
            }
        }

        public void RemoveAuthenticationCrate(ActionDTO actionDTO)
        {
            using (var updater = _crate.UpdateStorage(() => actionDTO.CrateStorage))
            {
                updater.CrateStorage.RemoveByManifestId((int) MT.StandardAuthentication);
            }
        }

        private void AddAuthenticationLabel(ActionDTO actionDTO)
        {
            using (var updater = _crate.UpdateStorage(actionDTO))
            {
                var controlsCrate = updater.CrateStorage
                    .CratesOfType<StandardConfigurationControlsCM>()
                    .FirstOrDefault();

                if (controlsCrate == null)
                {
                    controlsCrate = Crate<StandardConfigurationControlsCM>
                        .FromContent("Configuration_Controls", new StandardConfigurationControlsCM());

                    updater.CrateStorage.Add(controlsCrate);
                }

                controlsCrate.Content.Controls.Add(
                    new TextBlock()
                    {
                        Name = "AuthAwaitLabel",
                        Value = "Waiting for authentication window..."
                    });
            }
        }

        private void RemoveAuthenticationLabel(ActionDTO actionDTO)
        {
            using (var updater = _crate.UpdateStorage(actionDTO))
            {
                var controlsCrate = updater.CrateStorage
                    .CratesOfType<StandardConfigurationControlsCM>()
                    .FirstOrDefault();
                if (controlsCrate == null) { return; }

                var authAwaitLabel = controlsCrate.Content.FindByName("AuthAwaitLabel");
                if (authAwaitLabel == null) { return; }

                controlsCrate.Content.Controls.Remove(authAwaitLabel);

                if (controlsCrate.Content.Controls.Count == 0)
                {
                    updater.CrateStorage.Remove(controlsCrate);
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
                    RemoveAuthenticationCrate(curActionDTO);
                    RemoveAuthenticationLabel(curActionDTO);

                    var actionDO = uow.ActionRepository.GetByKey(curActionDTO.Id);
                    if (actionDO == null)
                    {
                        throw new NullReferenceException("Current action was not found.");
                    }

                    var authToken = actionDO.AuthorizationToken;

                    if (authToken == null || string.IsNullOrEmpty(authToken.Token))
                    {
                        AddAuthenticationCrate(curActionDTO, activityTemplate.AuthenticationType);
                        AddAuthenticationLabel(curActionDTO);

                        return true;
                    }
                }
            }

            return false;
        }

        public void InvalidateToken(string userId, ActionDTO curActionDto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository.GetByKey(curActionDto.ActivityTemplateId);

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
                    var actionDO = uow.ActionRepository.GetByKey(curActionDto.Id);
                    if (actionDO == null)
                    {
                        throw new NullReferenceException("Current action was not found.");
                    }

                    var token = actionDO.AuthorizationToken;

                    // var token = uow.AuthorizationTokenRepository
                    //     .FindOne(x => x.Terminal.Id == activityTemplate.Terminal.Id && x.UserDO.Id == account.Id);
                    
                    if (token != null)
                    {
                        actionDO.AuthorizationToken = null;
                        uow.SaveChanges();

                        uow.AuthorizationTokenRepository.Remove(token);
                        uow.SaveChanges();
                    }

                    RemoveAuthenticationCrate(curActionDto);
                    RemoveAuthenticationLabel(curActionDto);

                    AddAuthenticationCrate(curActionDto, activityTemplate.AuthenticationType);
                    AddAuthenticationLabel(curActionDto);
                }
            }
        }

        public IEnumerable<AuthorizationTokenDO> GetAllTokens(string accountId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authTokens = uow.AuthorizationTokenRepository
                    .GetQuery()
                    .Where(x => x.UserID == accountId)
                    .OrderBy(x => x.ExternalAccountId)
                    .ToList();

                return authTokens;
            }
        }

        public void RevokeToken(string accountId, Guid authTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .GetQuery()
                    .Where(x => x.UserID == accountId && x.Id == authTokenId)
                    .SingleOrDefault();

                if (authToken != null)
                {
                    var actions = uow.ActionRepository
                        .GetQuery()
                        .Where(x => x.AuthorizationToken.Id == authToken.Id)
                        .ToList();

                    foreach (var action in actions)
                    {
                        action.AuthorizationToken = null;
                    }

                    uow.SaveChanges();

                    uow.AuthorizationTokenRepository.Remove(authToken);
                    uow.SaveChanges();
                }
            }
        }
    }
}
