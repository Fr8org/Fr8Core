using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Services;
using HubWeb.Controllers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HubTests.Controllers.Api;
using Fr8.Testing.Unit.Fixtures;
using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Hub.Interfaces;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("AuthenticationController")]
    public class AuthenticationControllerTest : ApiControllerTestBase
    {
        private AuthenticationController _authenticationController;

        private readonly string Token = @"{""Email"":""64684b41-bdfd-4121-8f81-c825a6a03582"",""ApiPassword"":""HyCXOBeGl/Ted9zcMqd7YEKoN0Q=""}";

        public AuthenticationControllerTest()
        {
            base.SetUp();

            _authenticationController = CreateController<AuthenticationController>();
        }

        [Test]
        public void AuthenticationController_ShouldHaveFr8ApiAuthorizeOnAuthenticateMethod()
        {
            ShouldHaveFr8ApiAuthorizeOnFunction(typeof(AuthenticationController), "Authenticate");
        }

        [Test]
        public void AuthenticationController_ShouldHaveFr8ApiAuthorizeOnGetOAuthInitiationURLMethod()
        {
            ShouldHaveFr8ApiAuthorizeOnFunction(typeof(AuthenticationController), "GetOAuthInitiationURL");
        }

        private TerminalDO CreateAndAddTerminalDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalDO = new TerminalDO()
                {
                    Name = "terminalTest",
                    Label = "Test",
                    Version = "1",
                    TerminalStatus = 1,
                    Endpoint = "localhost:39504",
                    Secret = Guid.NewGuid().ToString(),
                    OperationalState = OperationalState.Active,
                    ParticipationState = ParticipationState.Approved
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

                tokenDO.Token = Token;
                uow.SaveChanges();

                return tokenDO;
            }
        }

        [Test]
        public async Task TestAuthenticate()
        {
            var tokenDO = CreateAndAddTokenDO();

            var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", tokenDO.TerminalID);
            activityTemplateDO.Id = FixtureData.GetTestGuidById(1);
            activityTemplateDO.Terminal = ObjectFactory.GetInstance<ITerminal>().GetByKey(tokenDO.TerminalID);
            activityTemplateDO.Terminal.AuthenticationType = AuthenticationType.Internal;

            var activityDO = FixtureData.TestActivity1();
            activityDO.ActivityTemplate = activityTemplateDO;
            // activityDO.AuthorizationToken = tokenDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    PlanState = PlanState.Executing,
                    ChildNodes = { activityDO }
                });

                //uow.ActivityRepository.Add(activityDO);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();
            }

            var credentialsDTO = new CredentialsDTO()
            {
                Password = "Password",
                Username = "Username",
                Domain = "Domain",
                Terminal = Mapper.Map<TerminalSummaryDTO>(activityTemplateDO.Terminal)
            };

            var result = _authenticationController.Authenticate(credentialsDTO);

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(
                ObjectFactory.GetInstance<IRestfulServiceClient>()
            );

            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync<CredentialsDTO>(
                    new Uri(activityTemplateDO.Terminal.Endpoint + "/authentication/token"),
                    It.Is<CredentialsDTO>(it => it.Username == credentialsDTO.Username
                        && it.Password == credentialsDTO.Password
                        && it.Domain == credentialsDTO.Domain), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()
                ),
                Times.Exactly(1)
            );


            restClientMock.VerifyAll();

        }
    }
}
