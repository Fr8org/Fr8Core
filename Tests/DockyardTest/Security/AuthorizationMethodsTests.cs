using System;
using Hub.Services;
using UtilitiesTesting;
using NUnit.Framework;
using Data.Entities;
using StructureMap;
using Data.Interfaces;
using Data.States;
using Data.Interfaces.DataTransferObjects;
using Owin;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Collections.Generic;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;

namespace DockyardTest.Security
{
    [TestFixture]
    [Category("Authorization")]
    public class AuthorizationMethodsTests : BaseTest
    {
        private Authorization _authorization;

        private readonly string Token = @"{""Email"":""64684b41-bdfd-4121-8f81-c825a6a03582"",""ApiPassword"":""HyCXOBeGl/Ted9zcMqd7YEKoN0Q=""}";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _authorization = new Authorization();
        }   

        private PluginDO CreateAndAddPluginDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var pluginDO = new PluginDO()
                {
                    Name = "terminalTest",
                    Version = "1",
                    PluginStatus = 1,
                    Endpoint = "localhost:39504"
                };

                uow.PluginRepository.Add(pluginDO);
                uow.SaveChanges();

                return pluginDO;
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
                var pluginDO = CreateAndAddPluginDO();
                var userDO = CreateAndAddUserDO();

                var tokenDO = new AuthorizationTokenDO()
                {
                    UserID = userDO.Id,
                    PluginID = pluginDO.Id,
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
            var testToken = _authorization.GetToken(tokenDO.UserDO.Id, tokenDO.PluginID);

            Assert.AreEqual(Token, testToken);
            
        }

        [Test]
        public void GetTokenByUserIdAndTerminalIdIsNull()
        {
            var token = _authorization.GetToken("null", 0);
            Assert.IsNull(token);
        }

        [Test]
        public void CanGetPluginToken()
        {
            var tokenDO = CreateAndAddTokenDO();
            var testToken = _authorization.GetPluginToken(tokenDO.PluginID);

            Assert.AreEqual(Token, testToken);            
        }

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
            var actionDTO = new ActionDTO();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", tokenDO.PluginID);
                activityTemplateDO.AuthenticationType = AuthenticationType.Internal; 
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();

                var processTemplateDO = new RouteDO()
                {
                    Id = 23,
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    RouteState = RouteState.Active,
                    Fr8Account = tokenDO.UserDO
                };
                uow.RouteRepository.Add(processTemplateDO);
                uow.SaveChanges();

                var actionDO = new ActionDO()
                {
                    ParentRouteNode = processTemplateDO,
                    ParentRouteNodeId = processTemplateDO.Id,
                    Name = "testaction",

                    Id = 1,
                    ActivityTemplateId = activityTemplateDO.Id,
                    ActivityTemplate = activityTemplateDO,
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
            var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", tokenDO.PluginID);
            activityTemplateDO.AuthenticationType = AuthenticationType.Internal;
            activityTemplateDO.Plugin = tokenDO.Plugin;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {   
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();
            }

            var credentialsDTO = new CredentialsDTO()
            {
                Username = "Username",
                Password = "Password"
            };

            var result = _authorization.AuthenticateInternal(
               tokenDO.UserDO, activityTemplateDO, credentialsDTO.Username, credentialsDTO.Password);

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(ObjectFactory.GetInstance<IRestfulServiceClient>());
                        
            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync<CredentialsDTO>(
                new Uri("http://" + activityTemplateDO.Plugin.Endpoint + "/authentication/internal"),
                It.Is < CredentialsDTO >(it=> it.Username ==  credentialsDTO.Username && it.Password == credentialsDTO.Password)), Times.Exactly(1));
                       

            restClientMock.VerifyAll();
        }


        [Test]
        public void CanGetOAuthToken()
        {
            var pluginDO = CreateAndAddPluginDO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", pluginDO.Id);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();
            }

            var externalAuthenticationDTO = new ExternalAuthenticationDTO()
            {
                RequestQueryString = "?id"
            };

            var result = _authorization.GetOAuthToken(pluginDO, externalAuthenticationDTO);

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(ObjectFactory.GetInstance<IRestfulServiceClient>());

            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync<ExternalAuthenticationDTO>(new Uri("http://" + pluginDO.Endpoint + "/authentication/token"),
                externalAuthenticationDTO), Times.Exactly(1));

            restClientMock.VerifyAll();


        }

        [Test]
        public void CanGetOAuthInitiationURL()
        {           
            var tokenDo = CreateAndAddTokenDO();

            var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", tokenDo.PluginID);
            activityTemplateDO.AuthenticationType = AuthenticationType.Internal;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {   
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();
            }

            var result = _authorization.GetOAuthInitiationURL(tokenDo.UserDO, activityTemplateDO);

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(ObjectFactory.GetInstance<IRestfulServiceClient>());

            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync(new Uri("http://" + tokenDo.Plugin.Endpoint + "/authentication/initial_url")), 
                    Times.Exactly(1));

            restClientMock.VerifyAll();
        }

        [Test]
        public void ValidateAuthenticationNeededIsTrue()
        {
            var userDO = CreateAndAddUserDO();
            var pluginDO = CreateAndAddPluginDO();
            var actionDTO = new ActionDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", pluginDO.Id);
                activityTemplateDO.AuthenticationType = AuthenticationType.Internal;
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
                uow.SaveChanges();

                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
            }
            var testResult = _authorization.ValidateAuthenticationNeeded(userDO.Id, actionDTO);

            Assert.IsTrue(testResult);
        }

        [Test]
        public void ValidateAuthenticationNeededIsFalse()
        {
            var tokenDO = CreateAndAddTokenDO();
            var actionDTO = new ActionDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", tokenDO.PluginID);
                activityTemplateDO.AuthenticationType = AuthenticationType.Internal;
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
                uow.SaveChanges();

                actionDTO.ActivityTemplateId = activityTemplateDO.Id;
            }
            var testResult = _authorization.ValidateAuthenticationNeeded(tokenDO.UserID, actionDTO);

            Assert.IsFalse(testResult);
        }
    }
}
