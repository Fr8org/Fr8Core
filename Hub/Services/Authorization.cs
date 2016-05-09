using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using Hub.Exceptions;
using Infrastructure.Communication;

namespace Hub.Services
{
    public class Authorization : IAuthorization
    {
        private readonly ICrateManager _crate;
        private readonly ITime _time;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ITerminal _terminal;


        public Authorization()
        {
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _time = ObjectFactory.GetInstance<ITime>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        public string GetToken(string userId, int terminalId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindToken(userId, terminalId, AuthorizationTokenState.Active);

                if (curAuthToken != null)
                    return curAuthToken.Token;
            }
            return null;
        }

        /// <summary>
        /// Prepare AuthToken for ActionDTO request message.
        /// </summary>
        public void PrepareAuthToken(IUnitOfWork uow, ActivityDTO activityDTO)
        {
            
            // Fetch Action.
            var activity = uow.PlanRepository.GetById<ActivityDO>(activityDTO.Id);
            if (activity == null)
            {
                throw new ApplicationException("Could not find Action.");
            }

            if (activity.ActivityTemplateId == null)
            {
                throw new ApplicationException("Activity without a template should not exist");
            }

            var activityTemplate = _activityTemplate.GetByKey(activity.ActivityTemplateId);        

            // Try to find AuthToken if terminal requires authentication.
            if (activityTemplate.NeedsAuthentication &&
                activityTemplate.Terminal.AuthenticationType != AuthenticationType.None)
            {
                AuthorizationTokenDO authToken =
                    uow.AuthorizationTokenRepository.FindTokenById(activity.AuthorizationTokenId);

                // If AuthToken is not empty, fill AuthToken property for ActionDTO.
                if (authToken != null && !string.IsNullOrEmpty(authToken.Token))
                {
                    activityDTO.AuthToken = new AuthorizationTokenDTO
                    {
                        Id = authToken.Id.ToString(),
                        ExternalAccountId = authToken.ExternalAccountId,
                        ExternalDomainId = authToken.ExternalDomainId,
                        UserId = authToken.UserID,
                        Token = authToken.Token,
                        AdditionalAttributes = authToken.AdditionalAttributes
                    };
                }
                else
                {
                    throw new InvalidTokenRuntimeException(activityDTO);
                }

                if (String.IsNullOrEmpty(authToken.Token))
                {
                    throw new InvalidTokenRuntimeException(activityDTO);
                }
            }

            if (activityDTO.AuthToken == null)
            {
                if (activity.Fr8AccountId != null)
                {
                    activityDTO.AuthToken = new AuthorizationTokenDTO
                    {
                        UserId = activity.Fr8AccountId,
                    };
                }
            }
        }

        public async Task<AuthenticateResponse> AuthenticateInternal(
            Fr8AccountDO account,
            TerminalDO terminal,
            string domain,
            string username,
            string password,
            bool isDemoAccount = false)
        {
            if (terminal.AuthenticationType == AuthenticationType.None)
            {
                throw new ApplicationException("Terminal does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new CredentialsDTO()
            {
                Domain = domain,
                Username = username,
                Password = password,
                IsDemoAccount = isDemoAccount,
                Fr8UserId = (account != null ? account.Id : null)
            };

            var terminalResponse = await restClient.PostAsync<CredentialsDTO>(
                new Uri(terminal.Endpoint + "/authentication/internal"),
                credentialsDTO
            );

            var terminalResponseAuthTokenDTO = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(terminalResponse);
            if (!string.IsNullOrEmpty(terminalResponseAuthTokenDTO.Error))
            {
                return new AuthenticateResponse()
                {
                    Error = terminalResponseAuthTokenDTO.Error
                };
            }

            if (terminalResponseAuthTokenDTO == null)
            {
                return new AuthenticateResponse()
                {
                    Error = "An error occured while authenticating, please try again."
                };
            }

            var curTerminal = _terminal.GetByKey(terminal.Id);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAccount = uow.UserRepository.GetByKey(account.Id);

                AuthorizationTokenDO authToken = null;
                if (!string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalAccountId))
                {
                    authToken = uow.AuthorizationTokenRepository
                        .GetPublicDataQuery()
                        .FirstOrDefault(x => x.TerminalID == curTerminal.Id
                            && x.UserID == curAccount.Id
                            && x.ExternalAccountId == terminalResponseAuthTokenDTO.ExternalAccountId
                            && x.AdditionalAttributes == terminalResponseAuthTokenDTO.AdditionalAttributes
                        );
                }


                var created = false;
                if (authToken == null)
                {
                    authToken = new AuthorizationTokenDO()
                    {
                        Token = terminalResponseAuthTokenDTO.Token,
                        ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId,
                        ExternalDomainId = terminalResponseAuthTokenDTO.ExternalDomainId,
                        TerminalID = curTerminal.Id,
                        UserDO = curAccount,
                        AdditionalAttributes = terminalResponseAuthTokenDTO.AdditionalAttributes,
                        ExpiresAt = DateTime.Today.AddMonths(1)
                    };

                    uow.AuthorizationTokenRepository.Add(authToken);
                    created = true;
                }
                else
                {
                    authToken.Token = terminalResponseAuthTokenDTO.Token;
                    authToken.ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId;
                }

                uow.SaveChanges();

                if (created)
                {
                    EventManager.AuthTokenCreated(authToken);
                }

                //if terminal requires Authentication Completed Notification, follow the existing terminal event notification protocol 
                //to notify the terminal about authentication completed event
                if (terminalResponseAuthTokenDTO.AuthCompletedNotificationRequired)
                {
                    //let's save id of DTO before informing related terminal
                    terminalResponseAuthTokenDTO.Id = authToken.Id.ToString();
                    EventManager.TerminalAuthenticationCompleted(curAccount.Id, curTerminal, terminalResponseAuthTokenDTO);
                }

                return new AuthenticateResponse()
                {
                    AuthorizationToken = Mapper.Map<AuthorizationTokenDTO>(authToken),
                    Error = null
                };
            }
        }

        public async Task<AuthenticateResponse> GetOAuthToken(
            TerminalDO terminal,
            ExternalAuthenticationDTO externalAuthDTO)
        {
            var hasAuthentication = _activityTemplate.GetQuery().Any(x => x.Terminal.Id == terminal.Id);

            if (!hasAuthentication)
            {
                throw new ApplicationException("Terminal does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync<ExternalAuthenticationDTO>(
                new Uri(terminal.Endpoint + "/authentication/token"),
                externalAuthDTO
                );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(response);
            if (!string.IsNullOrEmpty(authTokenDTO.Error))
            {
                return new AuthenticateResponse()
                {
                    Error = authTokenDTO.Error
                };
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authTokenByExternalState = uow.AuthorizationTokenRepository
                    .FindTokenByExternalState(authTokenDTO.ExternalStateToken, terminal.Id);

                if (authTokenByExternalState == null)
                {
                    throw new ApplicationException("No AuthToken found with specified ExternalStateToken.");
                }

                var authTokenByExternalAccountId = uow.AuthorizationTokenRepository
                    .FindTokenByExternalAccount(
                        authTokenDTO.ExternalAccountId,
                        terminal.Id,
                        authTokenByExternalState.UserID
                    );

                if (authTokenByExternalAccountId != null)
                {
                    authTokenByExternalAccountId.Token = authTokenDTO.Token;
                    authTokenByExternalState.ExternalAccountId = authTokenDTO.ExternalAccountId;
                    authTokenByExternalState.ExternalDomainId = authTokenDTO.ExternalDomainId;
                    authTokenByExternalAccountId.ExternalStateToken = null;
                    authTokenByExternalState.AdditionalAttributes = authTokenDTO.AdditionalAttributes;

                    uow.AuthorizationTokenRepository.Remove(authTokenByExternalState);

                    EventManager.AuthTokenCreated(authTokenByExternalAccountId);
                }
                else
                {
                    authTokenByExternalState.Token = authTokenDTO.Token;
                    authTokenByExternalState.ExternalAccountId = authTokenDTO.ExternalAccountId;
                    authTokenByExternalState.ExternalDomainId = authTokenDTO.ExternalDomainId;
                    authTokenByExternalState.ExternalStateToken = null;
                    authTokenByExternalState.AdditionalAttributes = authTokenDTO.AdditionalAttributes;

                    EventManager.AuthTokenCreated(authTokenByExternalState);
                }

                uow.SaveChanges();

                return new AuthenticateResponse()
                {
                    AuthorizationToken = Mapper.Map<AuthorizationTokenDTO>(authTokenByExternalAccountId ?? authTokenByExternalState),
                    Error = null
                };
            }
        }


        public async Task<ExternalAuthUrlDTO> GetOAuthInitiationURL(
            Fr8AccountDO user,
            TerminalDO terminal)
        {
            if (terminal.AuthenticationType == AuthenticationType.None)
            {
                throw new ApplicationException("Terminal does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync(
                new Uri(terminal.Endpoint + "/authentication/initial_url")
            );

            var externalAuthUrlDTO = JsonConvert.DeserializeObject<ExternalAuthUrlDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .GetPublicDataQuery()
                    .FirstOrDefault(x => x.TerminalID == terminal.Id
                        && x.UserID == user.Id
                        && x.ExternalAccountId == null
                        && x.ExternalStateToken != null
                    );

                if (authToken == null)
                {
                    var curTerminal = _terminal.GetByKey(terminal.Id);
                    var curAccount = uow.UserRepository.GetByKey(user.Id);

                    authToken = new AuthorizationTokenDO()
                    {
                        UserDO = curAccount,
                        TerminalID = curTerminal.Id,
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

        public void AddAuthenticationCrate(ActivityDTO activityDTO, int authType)
        {
            using (var crateStorage = _crate.UpdateStorage(() => activityDTO.CrateStorage))
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

                crateStorage.Add(_crate.CreateAuthenticationCrate("RequiresAuthentication", mode, false));
            }
        }

        public void RemoveAuthenticationCrate(ActivityDTO activityDTO)
        {
            using (var crateStorage = _crate.GetUpdatableStorage(activityDTO))
            {
                crateStorage.RemoveByManifestId((int)MT.StandardAuthentication);
            }
        }

        public void RevokeTokenIfNeeded(IUnitOfWork uow, ActivityDTO activityDTO)
        {
            using (var crateStorage = _crate.GetUpdatableStorage(activityDTO))
            {
                var authCM = crateStorage
                    .CrateContentsOfType<StandardAuthenticationCM>()
                    .FirstOrDefault();
                if (authCM != null && authCM.Revocation)
                {
                    var activity = uow.PlanRepository.GetById<ActivityDO>(activityDTO.Id);

                    if (activity.AuthorizationTokenId.HasValue)
                    {
                        RevokeToken(activity.Fr8AccountId, activity.AuthorizationTokenId.Value);
                    }
                }
            }
        }

        // TODO: FR-2703, remove this.
        // private void AddAuthenticationLabel(ActivityDTO activityDTO)
        // {
        //     using (var crateStorage = _crate.GetUpdatableStorage(activityDTO))
        //     {
        //         var controlsCrate = crateStorage
        //             .CratesOfType<StandardConfigurationControlsCM>()
        //             .FirstOrDefault();
        // 
        //         if (controlsCrate == null)
        //         {
        //             controlsCrate = Crate<StandardConfigurationControlsCM>
        //                 .FromContent("Configuration_Controls", new StandardConfigurationControlsCM());
        // 
        //             crateStorage.Add(controlsCrate);
        //         }
        // 
        //         controlsCrate.Content.Controls.Add(
        //             new TextBlock()
        //             {
        //                 Name = "AuthAwaitLabel",
        //                 Value = "Please provide credentials to access your desired account"
        //             });
        //     }
        // }

        // TODO: FR-2703, remove this.
        // private void RemoveAuthenticationLabel(ActivityDTO activityDTO)
        // {
        //     using (var crateStorage = _crate.GetUpdatableStorage(activityDTO))
        //     {
        //         var controlsCrate = crateStorage
        //             .CratesOfType<StandardConfigurationControlsCM>()
        //             .FirstOrDefault();
        //         if (controlsCrate == null) { return; }
        // 
        //         var authAwaitLabel = controlsCrate.Content.FindByName("AuthAwaitLabel");
        //         if (authAwaitLabel == null) { return; }
        // 
        //         controlsCrate.Content.Controls.Remove(authAwaitLabel);
        // 
        //         if (controlsCrate.Content.Controls.Count == 0)
        //         {
        //             crateStorage.Remove(controlsCrate);
        //         }
        //     }
        // }

        public bool ValidateAuthenticationNeeded(IUnitOfWork uow, string userId, ActivityDTO curActionDTO)
        {
            var activityTemplate = _activityTemplate.GetByNameAndVersion(curActionDTO.ActivityTemplate.Name, curActionDTO.ActivityTemplate.Version);

            if (activityTemplate == null)
            {
                throw new NullReferenceException("ActivityTemplate was not found.");
            }


            if (activityTemplate.Terminal.AuthenticationType != AuthenticationType.None
                && activityTemplate.NeedsAuthentication)
            {
                RemoveAuthenticationCrate(curActionDTO);
                // TODO: FR-2703, remove this.
                // RemoveAuthenticationLabel(curActionDTO);

                var activityDO = uow.PlanRepository.GetById<ActivityDO>(curActionDTO.Id);
                if (activityDO == null)
                {
                    throw new NullReferenceException("Current activity was not found.");
                }

                AuthorizationTokenDO authToken =
                    uow.AuthorizationTokenRepository.FindTokenById(activityDO.AuthorizationTokenId);

                // If action does not have assigned auth-token,
                // then look for AuthToken with IsMain == true,
                // and assign that token to action.
                if (authToken == null)
                {
                    var mainAuthTokenId = uow.AuthorizationTokenRepository
                        .GetPublicDataQuery()
                        .Where(x => x.UserID == userId
                                    && x.TerminalID == activityTemplate.Terminal.Id
                                    && x.IsMain == true)
                        .Select(x => (Guid?) x.Id)
                        .FirstOrDefault();

                    if (mainAuthTokenId.HasValue)
                    {
                        authToken = uow.AuthorizationTokenRepository
                            .FindTokenById(mainAuthTokenId);
                    }

                    if (authToken != null && !string.IsNullOrEmpty(authToken.Token))
                    {
                        activityDO.AuthorizationTokenId = authToken.Id;
                        uow.SaveChanges();
                    }
                }

                // FR-1958: remove token if could not extract secure data.
                if (authToken != null && string.IsNullOrEmpty(authToken.Token))
                {
                    EventManager.AuthTokenSilentRevoke(authToken);

                    RemoveToken(uow, authToken);
                    authToken = null;
                }

                if (authToken == null)
                {
                    AddAuthenticationCrate(curActionDTO, activityTemplate.Terminal.AuthenticationType);
                    // TODO: FR-2703, remove this.
                    // AddAuthenticationLabel(curActionDTO);

                    return true;
                }
            }


            return false;
        }

        public void InvalidateToken(IUnitOfWork uow, string userId, ActivityDTO curActivityDto)
        {
            var activityTemplate = _activityTemplate.GetByNameAndVersion(curActivityDto.ActivityTemplate.Name, curActivityDto.ActivityTemplate.Version);

            if (activityTemplate == null)
            {
                throw new NullReferenceException("ActivityTemplate was not found.");
            }

            if (activityTemplate.Terminal.AuthenticationType != AuthenticationType.None
                && activityTemplate.NeedsAuthentication)
            {
                var activityDO = uow.PlanRepository.GetById<ActivityDO>(curActivityDto.Id);
                if (activityDO == null)
                {
                    throw new NullReferenceException("Current activity was not found.");
                }

                var token = uow.AuthorizationTokenRepository.FindTokenById(activityDO.AuthorizationTokenId);

                RemoveToken(uow, token);

                RemoveAuthenticationCrate(curActivityDto);
                // TODO: FR-2703, remove this.
                // RemoveAuthenticationLabel(curActivityDto);

                AddAuthenticationCrate(curActivityDto, activityTemplate.Terminal.AuthenticationType);
                // TODO: FR-2703, remove this.
                // AddAuthenticationLabel(curActivityDto);
            }
        }

        public IEnumerable<AuthorizationTokenDO> GetAllTokens(string accountId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authTokens = uow.AuthorizationTokenRepository
                    .GetPublicDataQuery()
                    .Where(x => x.UserID == accountId)
                    .OrderBy(x => x.ExternalAccountId)
                    .ToList();

                return authTokens;
            }
        }

        public void GrantToken(Guid actionId, Guid authTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activity = uow.PlanRepository.GetById<ActivityDO>(actionId);
                if (activity == null)
                {
                    throw new ApplicationException("Could not find specified Action.");
                }

                var authToken = uow.AuthorizationTokenRepository.FindTokenById(authTokenId);
                if (authToken == null)
                {
                    throw new ApplicationException("Could not find specified AuthToken.");
                }

                activity.AuthorizationTokenId = authToken.Id;

                uow.SaveChanges();
            }
        }

        public void RevokeToken(string accountId, Guid authTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository.
                    GetPublicDataQuery().
                    SingleOrDefault(x => x.UserID == accountId && x.Id == authTokenId);

                if (authToken != null)
                {
                    RemoveToken(uow, authToken);
                }
            }
        }

        private void RemoveToken(IUnitOfWork uow, AuthorizationTokenDO authToken)
        {
            EventManager.AuthTokenRemoved(authToken);

            var activities = uow.PlanRepository.GetActivityQueryUncached()
                .Where(x => x.AuthorizationToken.Id == authToken.Id)
                .ToList();

            foreach (var activity in activities)
            {
                activity.AuthorizationToken = null;
                uow.PlanRepository.RemoveAuthorizationTokenFromCache(activity);
            }

            authToken = uow.AuthorizationTokenRepository.GetPublicDataQuery().FirstOrDefault(x => x.Id == authToken.Id);
            uow.AuthorizationTokenRepository.Remove(authToken);
            uow.SaveChanges();
        }

        public void SetMainToken(string userId, Guid authTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var mainAuthToken = uow.AuthorizationTokenRepository.GetPublicDataQuery().FirstOrDefault(x=>x.Id == authTokenId);

                if (mainAuthToken == null)
                {
                    throw new ApplicationException("Unable to find specified Auth-Token.");
                }

                var siblings = uow.AuthorizationTokenRepository
                    .GetPublicDataQuery()
                    .Where(x => x.UserID == userId && x.TerminalID == mainAuthToken.TerminalID);
                    

                foreach (var siblingAuthToken in siblings)
                {
                    siblingAuthToken.IsMain = false;
                }

                mainAuthToken.IsMain = true;

                uow.SaveChanges();
            }
        }
    }
}
