using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Hub.Interfaces;
using Newtonsoft.Json;
using StructureMap;
using Hub.Exceptions;

namespace Hub.Services
{
    public class Authorization : IAuthorization
    {
        private readonly ICrateManager _crate;
        private readonly ITime _time;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ITerminal _terminalService;

        public Authorization()
        {
            _terminalService = ObjectFactory.GetInstance<ITerminal>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _time = ObjectFactory.GetInstance<ITime>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        public string GetToken(string userId, Guid terminalId)
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
                AuthorizationTokenDO authToken;
                TryAssignAuthToken(uow, activity.Fr8AccountId, activityTemplate.TerminalId, activity, out authToken);

                // If AuthToken is not empty, fill AuthToken property for ActionDTO.
                if (authToken != null && !string.IsNullOrEmpty(authToken.Token))
                {
                    activityDTO.AuthToken = new AuthorizationTokenDTO
                    {
                        Id = authToken.Id.ToString(),
                        ExternalAccountId = authToken.ExternalAccountId,
                        ExternalAccountName = string.IsNullOrEmpty(authToken.ExternalAccountName) ? authToken.ExternalAccountId : authToken.ExternalAccountName,
                        ExternalDomainId = authToken.ExternalDomainId,
                        ExternalDomainName = string.IsNullOrEmpty(authToken.ExternalDomainName) ? authToken.ExternalDomainId : authToken.ExternalDomainName,
                        UserId = authToken.UserID,
                        Token = authToken.Token,
                        ExpiresAt = authToken.ExpiresAt,
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
                throw new WrongAuthenticationTypeException();
            }
            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new CredentialsDTO
            {
                Domain = domain,
                Username = username,
                Password = password,
                IsDemoAccount = isDemoAccount,
                Fr8UserId = account?.Id
            };

            var terminalResponse = await restClient.PostAsync(
                new Uri(terminal.Endpoint + "/authentication/token"),
                credentialsDTO,
                null,
                _terminalService.GetRequestHeaders(terminal, account?.Id));

            var terminalResponseAuthTokenDTO = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(terminalResponse);
            if (!string.IsNullOrEmpty(terminalResponseAuthTokenDTO.Error))
            {
                return new AuthenticateResponse
                {
                    Error = terminalResponseAuthTokenDTO.Error
                };
            }

            var curTerminal = _terminalService.GetByKey(terminal.Id);

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
                            && x.ExternalDomainId == terminalResponseAuthTokenDTO.ExternalDomainId
                            && x.ExternalAccountId == terminalResponseAuthTokenDTO.ExternalAccountId
                            && x.AdditionalAttributes == terminalResponseAuthTokenDTO.AdditionalAttributes
                        );
                }


                var created = false;
                if (authToken == null)
                {
                    authToken = new AuthorizationTokenDO
                    {
                        Token = terminalResponseAuthTokenDTO.Token,
                        ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId,
                        ExternalAccountName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalAccountName) ? terminalResponseAuthTokenDTO.ExternalAccountId : terminalResponseAuthTokenDTO.ExternalAccountName,
                        ExternalDomainId = terminalResponseAuthTokenDTO.ExternalDomainId,
                        ExternalDomainName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalDomainName) ? terminalResponseAuthTokenDTO.ExternalDomainId : terminalResponseAuthTokenDTO.ExternalDomainName,
                        TerminalID = curTerminal.Id,
                        UserID = curAccount.Id,
                        AdditionalAttributes = terminalResponseAuthTokenDTO.AdditionalAttributes,
                        ExpiresAt = terminalResponseAuthTokenDTO.ExpiresAt
                    };

                    uow.AuthorizationTokenRepository.Add(authToken);
                    created = true;
                }
                else
                {
                    authToken.Token = terminalResponseAuthTokenDTO.Token;
                    authToken.ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId;
                    authToken.ExternalAccountName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalAccountName) ? terminalResponseAuthTokenDTO.ExternalDomainId : terminalResponseAuthTokenDTO.ExternalAccountName;
                    authToken.ExternalDomainId = terminalResponseAuthTokenDTO.ExternalDomainId;
                    authToken.ExternalDomainName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalDomainName) ? terminalResponseAuthTokenDTO.ExternalDomainId : terminalResponseAuthTokenDTO.ExternalDomainName;
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

                return new AuthenticateResponse
                {
                    AuthorizationToken = Mapper.Map<AuthorizationTokenDTO>(authToken),
                    Error = null
                };
            }
        }


        public async Task<AuthenticateResponse> GetOAuthToken(TerminalDO terminal, ExternalAuthenticationDTO externalAuthDTO)
        {
            var hasAuthentication = _activityTemplate.GetQuery().Any(x => x.Terminal.Id == terminal.Id);

            if (!hasAuthentication)
            {
                throw new WrongAuthenticationTypeException();
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            var response = await restClient.PostAsync(
                new Uri(terminal.Endpoint + "/authentication/token"),
                externalAuthDTO, null, _terminalService.GetRequestHeaders(terminal, null));

            var authTokenDTO = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(response);
            if (!string.IsNullOrEmpty(authTokenDTO.Error))
            {
                return new AuthenticateResponse
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
                    throw new MissingObjectException($"Authorization token with external state '{authTokenDTO.ExternalStateToken}' doesn't exist");
                }

                var authTokenByExternalAccountId = uow.AuthorizationTokenRepository
                    .FindTokenByExternalAccount(
                        authTokenDTO.ExternalAccountId,
                        terminal.Id,
                        authTokenByExternalState.UserID);

                if (authTokenByExternalAccountId != null)
                {
                    authTokenByExternalAccountId.Token = authTokenDTO.Token;
                    authTokenByExternalState.ExternalAccountId = authTokenDTO.ExternalAccountId;
                    authTokenByExternalState.ExternalAccountName = string.IsNullOrEmpty(authTokenDTO.ExternalAccountName) ? authTokenDTO.ExternalAccountId : authTokenDTO.ExternalAccountName;
                    authTokenByExternalState.ExternalDomainId = authTokenDTO.ExternalDomainId;
                    authTokenByExternalState.ExternalDomainName = string.IsNullOrEmpty(authTokenDTO.ExternalDomainName) ? authTokenDTO.ExternalDomainId : authTokenDTO.ExternalDomainName;
                    authTokenByExternalAccountId.ExternalStateToken = null;
                    authTokenByExternalState.AdditionalAttributes = authTokenDTO.AdditionalAttributes;
                    authTokenByExternalState.ExpiresAt = authTokenDTO.ExpiresAt;
                    if (authTokenByExternalState != null)
                    {
                        uow.AuthorizationTokenRepository.Remove(authTokenByExternalState);
                    }

                    EventManager.AuthTokenCreated(authTokenByExternalAccountId);
                }
                else
                {
                    authTokenByExternalState.Token = authTokenDTO.Token;
                    authTokenByExternalState.ExternalAccountId = authTokenDTO.ExternalAccountId;
                    authTokenByExternalState.ExternalAccountName = string.IsNullOrEmpty(authTokenDTO.ExternalAccountName) ? authTokenDTO.ExternalAccountId : authTokenDTO.ExternalAccountName;
                    authTokenByExternalState.ExternalDomainId = authTokenDTO.ExternalDomainId;
                    authTokenByExternalState.ExternalDomainName = string.IsNullOrEmpty(authTokenDTO.ExternalDomainName) ? authTokenDTO.ExternalDomainId : authTokenDTO.ExternalDomainName;
                    authTokenByExternalState.ExternalStateToken = null;
                    authTokenByExternalState.AdditionalAttributes = authTokenDTO.AdditionalAttributes;
                    authTokenByExternalState.ExpiresAt = authTokenDTO.ExpiresAt;

                    EventManager.AuthTokenCreated(authTokenByExternalState);
                }

                uow.SaveChanges();

                return new AuthenticateResponse
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
                throw new WrongAuthenticationTypeException();
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync(
                new Uri(terminal.Endpoint + "/authentication/request_url"),
                null,
                _terminalService.GetRequestHeaders(terminal, user.Id));

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
                    var curTerminal = _terminalService.GetByKey(terminal.Id);
                    var curAccount = uow.UserRepository.GetByKey(user.Id);

                    authToken = new AuthorizationTokenDO
                    {
                        UserID = curAccount.Id,
                        TerminalID = curTerminal.Id,
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
                var mode = authType == AuthenticationType.Internal ? AuthenticationMode.InternalMode : AuthenticationMode.ExternalMode;

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
                    default:
                        mode = AuthenticationMode.ExternalMode;
                        break;
                }

                crateStorage.Add("RequiresAuthentication", new StandardAuthenticationCM
                {
                    Mode = mode,
                    Revocation = false
                });
            }
        }

        public void RemoveAuthenticationCrate(ActivityDTO activityDTO)
        {
            using (var crateStorage = _crate.GetUpdatableStorage(activityDTO))
            {
                crateStorage.RemoveByManifestId((int)Fr8.Infrastructure.Data.Constants.MT.StandardAuthentication);
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

        public bool ValidateAuthenticationNeeded(IUnitOfWork uow, string userId, ActivityDTO activityDTO)
        {
            var activityTemplate = _activityTemplate.GetByNameAndVersion(activityDTO.ActivityTemplate);

            if (activityTemplate == null)
            {
                throw new MissingObjectException($"Activity template with name '{activityDTO.ActivityTemplate.Name}' and version '{activityDTO.ActivityTemplate.Version}' doesn't exist");
            }

            var activityDO = uow.PlanRepository.GetById<ActivityDO>(activityDTO.Id);
            if (activityDO == null)
            {
                throw new MissingObjectException($"Activity with Id {activityDTO.Id} doesn't exist");
            }

            if (activityTemplate.Terminal.AuthenticationType != AuthenticationType.None
                && activityTemplate.NeedsAuthentication)
            {
                RemoveAuthenticationCrate(activityDTO);

                AuthorizationTokenDO authToken;
                TryAssignAuthToken(uow, userId, activityTemplate.TerminalId, activityDO, out authToken);

                // FR-1958: remove token if could not extract secure data.
                if (authToken != null && string.IsNullOrEmpty(authToken.Token))
                {
                    EventManager.AuthTokenSilentRevoke(authToken);

                    RemoveToken(uow, authToken);
                    authToken = null;
                }

                if (authToken == null)
                {
                    AddAuthenticationCrate(activityDTO, activityTemplate.Terminal.AuthenticationType);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to get authorization token by terminal Id and userId. If token is not
        /// associated with the supplied activityId, an attempt to look up the main token for the specified 
        /// terminal will be done. If the main token is found, it will be assigned to the supplied ActivityDO.
        /// </summary>
        /// <returns>true if token is found. false, if not.</returns>
        public bool TryAssignAuthToken(IUnitOfWork uow, string userId, Guid terminalId, ActivityDO activityDO, out AuthorizationTokenDO curAuthToken)
        {
            curAuthToken = uow.AuthorizationTokenRepository.FindTokenById(activityDO.AuthorizationTokenId);

            // If action does not have assigned auth-token,
            // then look for AuthToken with IsMain == true,
            // and assign that token to action.
            if (curAuthToken == null)
            {
                var mainAuthTokenId = uow.AuthorizationTokenRepository
                    .GetPublicDataQuery()
                    .Where(x => x.UserID == userId
                                && x.TerminalID == terminalId
                                && x.IsMain)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefault();

                if (mainAuthTokenId.HasValue)
                {
                    curAuthToken = uow.AuthorizationTokenRepository
                        .FindTokenById(mainAuthTokenId);
                }

                if (!string.IsNullOrEmpty(curAuthToken?.Token))
                {
                    activityDO.AuthorizationTokenId = curAuthToken.Id;
                    uow.SaveChanges();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void InvalidateToken(IUnitOfWork uow, string userId, ActivityDTO curActivityDto)
        {
            var activityTemplate = _activityTemplate.GetByNameAndVersion(curActivityDto.ActivityTemplate);

            if (activityTemplate == null)
            {
                throw new MissingObjectException($"Activity template with name '{curActivityDto.ActivityTemplate.Name}' and version '{curActivityDto.ActivityTemplate.Version}' doesn't exist");
            }

            if (activityTemplate.Terminal.AuthenticationType != AuthenticationType.None
                && activityTemplate.NeedsAuthentication)
            {
                var activityDO = uow.PlanRepository.GetById<ActivityDO>(curActivityDto.Id);
                if (activityDO == null)
                {
                    throw new MissingObjectException($"Activity with Id {curActivityDto.Id} doesn't exist");
                }

                var token = uow.AuthorizationTokenRepository.FindTokenById(activityDO.AuthorizationTokenId);

                RemoveToken(uow, token);

                RemoveAuthenticationCrate(curActivityDto);
                AddAuthenticationCrate(curActivityDto, activityTemplate.Terminal.AuthenticationType);
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
                    throw new MissingObjectException($"Activity with Id {actionId} doesn't exist");
                }

                var authToken = uow.AuthorizationTokenRepository.FindTokenById(authTokenId);
                if (authToken == null)
                {
                    throw new MissingObjectException($"Authorization token with Id {authTokenId} doesn't exist");
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

        /// <summary>
        /// Not all fields of token will be replaced in database!
        /// </summary>
        /// <param name="token"></param>
        public void RenewToken(AuthorizationTokenDTO token)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository.FindTokenById(Guid.Parse(token.Id));
                if (authToken == null)
                {
                    return;
                }
                authToken.ExternalAccountId = token.ExternalAccountId;
                authToken.Token = token.Token;
                authToken.ExpiresAt = token.ExpiresAt;
                authToken.AdditionalAttributes = token.AdditionalAttributes;

                uow.SaveChanges();
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
            if (authToken != null)
            {
                uow.AuthorizationTokenRepository.Remove(authToken);
            }

            //Deactivating active related plans
            var _plan = ObjectFactory.GetInstance<IPlan>();
            var plans = new List<PlanDO>();
            foreach (var activity in activities)
            {
                //if template has Monitor category
                if (activity.ActivityTemplate.Categories.Where(a => a.ActivityCategoryId == ActivityCategories.MonitorId).FirstOrDefault() != null)
                {
                    _plan.Deactivate(activity.RootPlanNodeId.Value);
                }
            }

            uow.SaveChanges();
        }

        public void SetMainToken(string userId, Guid authTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var mainAuthToken = uow.AuthorizationTokenRepository.GetPublicDataQuery().FirstOrDefault(x => x.Id == authTokenId);

                if (mainAuthToken == null)
                {
                    throw new MissingObjectException($"Authorization token with Id {authTokenId} doesn't exist");
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

        public async Task<AuthenticateResponse> VerifyCodeAndGetAccessToken(
          Fr8AccountDO account,
          TerminalDO terminal,
          string phoneNumber,
          string verificationCode,
          string clientId,
          string clientName)
        {
            if (terminal.AuthenticationType == AuthenticationType.None)
            {
                throw new WrongAuthenticationTypeException();
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new PhoneNumberCredentialsDTO()
            {
                PhoneNumber = phoneNumber,
                ClientId = clientId,
                VerificationCode = verificationCode,
                Fr8UserId = account?.Id,
                ClientName = clientName
            };

            var terminalResponse = await restClient.PostAsync(new Uri(terminal.Endpoint + "/authentication/token"), credentialsDTO);

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

            var curTerminal = _terminalService.GetByKey(terminal.Id);

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
                            && x.ExternalDomainId == terminalResponseAuthTokenDTO.ExternalDomainId
                            && x.ExternalAccountId == terminalResponseAuthTokenDTO.ExternalAccountId
                            && x.AdditionalAttributes == terminalResponseAuthTokenDTO.AdditionalAttributes);
                }
                var created = false;
                if (authToken == null)
                {
                    authToken = new AuthorizationTokenDO()
                    {
                        Token = terminalResponseAuthTokenDTO.Token,
                        ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId,
                        ExternalAccountName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalAccountName) ? terminalResponseAuthTokenDTO.ExternalAccountId : terminalResponseAuthTokenDTO.ExternalAccountName,
                        ExternalDomainId = terminalResponseAuthTokenDTO.ExternalDomainId,
                        ExternalDomainName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalDomainName) ? terminalResponseAuthTokenDTO.ExternalDomainId : terminalResponseAuthTokenDTO.ExternalDomainName,
                        TerminalID = curTerminal.Id,
                        UserID = curAccount.Id,
                        AdditionalAttributes = terminalResponseAuthTokenDTO.AdditionalAttributes,
                    };

                    uow.AuthorizationTokenRepository.Add(authToken);
                    created = true;
                }
                else
                {
                    authToken.Token = terminalResponseAuthTokenDTO.Token;
                    authToken.ExternalAccountId = terminalResponseAuthTokenDTO.ExternalAccountId;
                    authToken.ExternalAccountName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalAccountName) ? terminalResponseAuthTokenDTO.ExternalDomainId : terminalResponseAuthTokenDTO.ExternalAccountName;
                    authToken.ExternalDomainId = terminalResponseAuthTokenDTO.ExternalDomainId;
                    authToken.ExternalDomainName = string.IsNullOrEmpty(terminalResponseAuthTokenDTO.ExternalDomainName) ? terminalResponseAuthTokenDTO.ExternalDomainId : terminalResponseAuthTokenDTO.ExternalDomainName;
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

        public async Task<PhoneNumberCredentialsDTO> SendAuthenticationCodeToMobilePhone(Fr8AccountDO account, TerminalDO terminal, string phoneNumber)
        {
            if (terminal.AuthenticationType == AuthenticationType.None)
            {
                throw new WrongAuthenticationTypeException();
            }

            if (terminal.AuthenticationType != AuthenticationType.PhoneNumberWithCode)
            {
                throw new WrongAuthenticationTypeException("Terminal support only authentication through phone number");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new PhoneNumberCredentialsDTO
            {
                PhoneNumber = phoneNumber,
                ClientName = account != null ? account.UserName : "Fr8 Client Name",
                Fr8UserId = account?.Id
            };

            var terminalResponse = await restClient.PostAsync(new Uri(terminal.Endpoint + "/authentication/send_code"), credentialsDTO);

            //response provides terminal 
            var terminalResponseContent = JsonConvert.DeserializeObject<PhoneNumberCredentialsDTO>(terminalResponse);

            return terminalResponseContent;
        }
    }
}
