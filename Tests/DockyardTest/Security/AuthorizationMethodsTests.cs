using System;
using Hub.Services;
using NUnit.Framework;
using Data.Entities;
using StructureMap;
using Data.Interfaces;
using Data.States;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Owin;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Collections.Generic;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using Hub.Managers;
using Data.Interfaces.Manifests;
using Data.Crates;

namespace DockyardTest.Security
{
    [TestFixture]
    [Category("Authorization")]
    public class AuthorizationMethodsTests : BaseTest
    {
        private Authorization _authorization;

        private ICrateManager _crate;

        private readonly string Token = @"{""Email"":""64684b41-bdfd-4121-8f81-c825a6a03582"",""ApiPassword"":""HyCXOBeGl/Ted9zcMqd7YEKoN0Q=""}";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _authorization = new Authorization();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }   

        private TerminalDO CreateAndAddTerminalDO(int authType = AuthenticationType.None)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalDO = new TerminalDO()
                {
                    Name = "terminalTest",
                    Version = "1",
                    TerminalStatus = 1,
                    Endpoint = "localhost:39504",
                    AuthenticationType = authType
                };

                uow.TerminalRepository.Add(terminalDO);
                uow.SaveChanges();

                return terminalDO;
            }
        }

        private Fr8AccountDO CreateAndAddUserDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = new Fr8Account();
                var emailAddress = new EmailAddressDO
                {
                    Address = "tester@gmail.com",
                    Name = "Test Tester"
                };

                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);
                uow.SaveChanges();

                return userDO;
            }
        }

        private AuthorizationTokenDO CreateAndAddTokenDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalDO = CreateAndAddTerminalDO();
                var userDO = CreateAndAddUserDO();

                var tokenDO = new AuthorizationTokenDO()
                {
                    UserID = userDO.Id,
                    TerminalID = terminalDO.Id,
                    AuthorizationTokenState = AuthorizationTokenState.Active
                };
                uow.AuthorizationTokenRepository.Add(tokenDO);

                tokenDO.ExpiresAt = DateTime.UtcNow.AddYears(100);
                tokenDO.Token = Token;
                uow.SaveChanges();

                return tokenDO;
            }
        }

        [Test]
        public void CanGetTokenByUserId()
        {    
            var tokenDO = CreateAndAddTokenDO();
            var testToken = _authorization.GetToken(tokenDO.UserDO.Id);

            Assert.AreEqual(Token, testToken);
        }

        [Test]
        public void GetTokenByUserIdIsNull()
        {
            var token = _authorization.GetToken("null");
            Assert.IsNull(token);
        }

        [Test]
        public void CanGetTokenByUserIdAndTerminalId()
        {
            var tokenDO = CreateAndAddTokenDO();
            var testToken = _authorization.GetToken(tokenDO.UserDO.Id, tokenDO.TerminalID);

            Assert.AreEqual(Token, testToken);
            
        }

        [Test]
        public void GetTokenByUserIdAndTerminalIdIsNull()
        {
            var token = _authorization.GetToken("null", 0);
            Assert.IsNull(token);
        }

//        [Test]
//        public void CanGetTerminalToken()
//        {
//            var tokenDO = CreateAndAddTokenDO();
//            var testToken = _authorization.GetTerminalToken(tokenDO.TerminalID);
//
//            Assert.AreEqual(Token, testToken);            
//        }

        [Test]
        public void CanUpdateToken()
        {
            var tokenDO = CreateAndAddTokenDO();
            var newToken = Token + "new";
            _authorization.AddOrUpdateToken(tokenDO.UserID, newToken);

            Assert.AreEqual(newToken, tokenDO.Token);
        }


        [Test]
        public void CanRemoveToken()
        {
            var tokenDO = CreateAndAddTokenDO();
            var userId = tokenDO.UserID;

            _authorization.RemoveToken(userId);

            var testToken = _authorization.GetToken(userId);

            Assert.IsNullOrEmpty(testToken);
        }

        [Test]
        public void CanPrepareAuthToken()
        {
            var tokenDO = CreateAndAddTokenDO();
            tokenDO.Terminal.AuthenticationType = AuthenticationType.Internal;

            var actionDTO = new ActionDTO();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO(
                    "test_name",
                    "test_label",
                    "1",
                    "test_description",
                    tokenDO.TerminalID
                );
                activityTemplateDO.NeedsAuthentication = true;
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();

                var routeDO = new RouteDO()
                {
                    Id = FixtureData.GetTestGuidById(23),
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    RouteState = RouteState.Active,
                    Fr8Account = tokenDO.UserDO
                };
                uow.RouteRepository.Add(routeDO);
                uow.SaveChanges();

                var actionDO = new ActionDO()
                {
                    ParentRouteNode = routeDO,
                    ParentRouteNodeId = routeDO.Id,
                    Name = "testaction",

                    Id = FixtureData.GetTestGuidById(1),
                    ActivityTemplateId = activityTemplateDO.Id,
                    ActivityTemplate = activityTemplateDO,
                    AuthorizationToken = tokenDO,
                    Ordering = 1
                };

                uow.ActionRepository.Add(actionDO);
                uow.SaveChanges();

                actionDTO.Id = actionDO.Id;
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
            }
            
                
            _authorization.PrepareAuthToken(actionDTO);

            Assert.AreEqual(Token, actionDTO.AuthToken.Token);
        }

        [Test]
        public void CanAuthenticateInternal()
        {   
            var tokenDO = CreateAndAddTokenDO();
            var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", tokenDO.TerminalID);
            activityTemplateDO.Terminal = tokenDO.Terminal;
            activityTemplateDO.Terminal.AuthenticationType = AuthenticationType.Internal;

            var actionDO = FixtureData.TestAction1();
            actionDO.ActivityTemplate = activityTemplateDO;
            actionDO.AuthorizationToken = tokenDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {   
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.RouteNodeRepository.Add(actionDO);
                uow.SaveChanges();
            }

            var credentialsDTO = new CredentialsDTO()
            {
                Username = "Username",
                Password = "Password",
                Domain = "Domain"
            };

            var result = _authorization.AuthenticateInternal(
               tokenDO.UserDO,
               tokenDO.Terminal,
               credentialsDTO.Domain,
               credentialsDTO.Username,
               credentialsDTO.Password
            );

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(
                ObjectFactory.GetInstance<IRestfulServiceClient>()
            );
                        
            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync<CredentialsDTO>(
                new Uri("http://" + activityTemplateDO.Terminal.Endpoint + "/authentication/internal"),
                It.Is < CredentialsDTO >(it=> it.Username ==  credentialsDTO.Username && 
                                              it.Password == credentialsDTO.Password &&
                                              it.Domain == credentialsDTO.Domain)), Times.Exactly(1));
                       

            restClientMock.VerifyAll();
        }


        [Test]
        public void CanGetOAuthToken()
        {
            var terminalDO = CreateAndAddTerminalDO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", terminalDO.Id);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();
            }

            var externalAuthenticationDTO = new ExternalAuthenticationDTO()
            {
                RequestQueryString = "?id"
            };

            var result = _authorization.GetOAuthToken(terminalDO, externalAuthenticationDTO);

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(ObjectFactory.GetInstance<IRestfulServiceClient>());

            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync<ExternalAuthenticationDTO>(new Uri("http://" + terminalDO.Endpoint + "/authentication/token"),
                externalAuthenticationDTO), Times.Exactly(1));

            restClientMock.VerifyAll();


        }

        [Test]
        public void CanGetOAuthInitiationURL()
        {
            var tokenDO = CreateAndAddTokenDO();
            tokenDO.Terminal.AuthenticationType = AuthenticationType.Internal;

            var activityTemplateDO = new ActivityTemplateDO(
                "test_name", "test_label", "1", "test_description", tokenDO.TerminalID
            );

            var actionDO = FixtureData.TestAction1();
            actionDO.ActivityTemplate = activityTemplateDO;
            actionDO.AuthorizationToken = tokenDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {   
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.RouteNodeRepository.Add(actionDO);
                uow.SaveChanges();
            }

            var result = _authorization.GetOAuthInitiationURL(tokenDO.UserDO, tokenDO.Terminal);

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(ObjectFactory.GetInstance<IRestfulServiceClient>());

            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync(
                    new Uri("http://" + tokenDO.Terminal.Endpoint + "/authentication/initial_url")
                ), 
                Times.Exactly(1)
            );

            restClientMock.VerifyAll();
        }

        [Test]
        public void ValidateAuthenticationNeededIsTrue()
        {
            var userDO = CreateAndAddUserDO();
            
            var terminalDO = CreateAndAddTerminalDO(AuthenticationType.Internal);

            var actionDO = FixtureData.TestAction1();
            var actionDTO = new ActionDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO(
                    "test_name",
                    "test_label",
                    "1",
                    "test_description",
                    terminalDO.Id
                );

                activityTemplateDO.NeedsAuthentication = true;

                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;

                actionDO.ActivityTemplate = activityTemplateDO;
                uow.ActionRepository.Add(actionDO);

                uow.SaveChanges();

                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
                actionDTO.Id = actionDO.Id;
            }
            var testResult = _authorization.ValidateAuthenticationNeeded(userDO.Id, actionDTO);

            Assert.IsTrue(testResult);
        }

        [Test]
        public void ValidateAuthenticationNeededIsFalse()
        {
            var tokenDO = CreateAndAddTokenDO();
            tokenDO.Terminal.AuthenticationType = AuthenticationType.Internal;

            var actionDO = FixtureData.TestAction1();
            var actionDTO = new ActionDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", tokenDO.TerminalID);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;

                actionDO.ActivityTemplate = activityTemplateDO;
                actionDO.AuthorizationToken = tokenDO;
                uow.ActionRepository.Add(actionDO);

                uow.SaveChanges();

                actionDTO.Id = actionDO.Id;
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
            }

            var testResult = _authorization.ValidateAuthenticationNeeded(tokenDO.UserID, actionDTO);

            Assert.IsFalse(testResult);
        }

        [Test]
        public void TestAddAuthenticationCrate()
        {
            var userDO = CreateAndAddUserDO();
            var terminalDO = CreateAndAddTerminalDO();
            terminalDO.AuthenticationType = AuthenticationType.Internal;

            var actionDTO = new ActionDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", terminalDO.Id);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
                uow.SaveChanges();

                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
            }

            
            _authorization.AddAuthenticationCrate(actionDTO, AuthenticationType.Internal);
            Assert.IsTrue(IsCratePresents(actionDTO, AuthenticationMode.InternalMode));

            _authorization.AddAuthenticationCrate(actionDTO, AuthenticationType.External);
            Assert.IsTrue(IsCratePresents(actionDTO, AuthenticationMode.ExternalMode));

            _authorization.AddAuthenticationCrate(actionDTO, AuthenticationType.InternalWithDomain);
            Assert.IsTrue(IsCratePresents(actionDTO, AuthenticationMode.InternalModeWithDomain));
        }

        private bool IsCratePresents(ActionDTO actionDTO, AuthenticationMode mode)
        {
            var result = false;
            foreach (var crate in actionDTO.CrateStorage.Crates)
            {
                if ( (int)mode == crate.Contents["Mode"].ToObject<int>())
                {
                    result = true;
                    break;
                }
               
            }

            return result;
        }
    }
}
