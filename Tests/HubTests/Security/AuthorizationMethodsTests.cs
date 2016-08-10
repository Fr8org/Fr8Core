using System;
using Hub.Services;
using NUnit.Framework;
using Data.Entities;
using StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using System.Collections.Generic;
using Moq;
using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Hub.Interfaces;

namespace HubTests.Security
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
                    Label = "Test",
                    Version = "1",
                    TerminalStatus = 1,
                    Endpoint = "localhost:39504",
                    AuthenticationType = authType,
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
        public void CanGetTokenByUserIdAndTerminalId()
        {
            var tokenDO = CreateAndAddTokenDO();
            var testToken = _authorization.GetToken(tokenDO.UserID, tokenDO.TerminalID);

            Assert.AreEqual(Token, testToken);
        }

        [Test]
        public void GetTokenByUserIdAndTerminalIdIsNull()
        {
            var token = _authorization.GetToken("null", Guid.Empty);
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
        public void CanPrepareAuthToken()
        {
            var tokenDO = CreateAndAddTokenDO();
           

            var activityDTO = new ActivityDTO();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO(
                    "test_name",
                    "test_label",
                    "1",
                    "test_description",
                    tokenDO.TerminalID
                );

                uow.TerminalRepository.GetByKey(tokenDO.TerminalID).AuthenticationType = AuthenticationType.Internal;
                activityTemplateDO.NeedsAuthentication = true;
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();

                var planDO = new PlanDO()
                {
                    Id = FixtureData.GetTestGuidById(23),
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    PlanState = PlanState.Executing,
                    Fr8Account = uow.UserRepository.GetByKey(tokenDO.UserID)
                };
                uow.PlanRepository.Add(planDO);
                uow.SaveChanges();

                var activityDO = new ActivityDO()
                {
                    ParentPlanNode = planDO,
                    ParentPlanNodeId = planDO.Id,
                    Id = FixtureData.GetTestGuidById(1),
                    ActivityTemplateId = activityTemplateDO.Id,
                    ActivityTemplate = activityTemplateDO,
                    AuthorizationTokenId = tokenDO.Id,
                    AuthorizationToken = tokenDO,
                    Ordering = 1
                };

                planDO.ChildNodes.Add(activityDO);

                // uow.ActivityRepository.Add(activityDO);
                uow.SaveChanges();

                activityDTO.Id = activityDO.Id;
                activityDTO.ActivityTemplate = Mapper.Map<ActivityTemplateSummaryDTO>(activityTemplateDO);
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _authorization.PrepareAuthToken(uow, activityDTO);
            }

            Assert.AreEqual(Token, activityDTO.AuthToken.Token);
        }

        [Test]
        public void CanAuthenticateInternal()
        {
            var tokenDO = CreateAndAddTokenDO();
            var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", tokenDO.TerminalID);
            activityTemplateDO.Id = FixtureData.GetTestGuidById(1);
            activityTemplateDO.Terminal = ObjectFactory.GetInstance<ITerminal>().GetByKey(tokenDO.TerminalID);

            var activityDO = FixtureData.TestActivity1();
            activityDO.ActivityTemplate = activityTemplateDO;
            activityDO.AuthorizationToken = tokenDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.TerminalRepository.GetByKey(tokenDO.TerminalID).AuthenticationType = AuthenticationType.Internal;
                uow.ActivityTemplateRepository.Add(activityTemplateDO);

                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    PlanState = PlanState.Executing,
                    ChildNodes = { activityDO }
                });

                uow.SaveChanges();
            }

            var credentialsDTO = new CredentialsDTO()
            {
                Username = "Username",
                Password = "Password",
                Domain = "Domain"
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.GetByKey(tokenDO.UserID);
                var terminal = uow.TerminalRepository.GetByKey(tokenDO.TerminalID);

                var result = _authorization.AuthenticateInternal(
                    user,
                    terminal,
                    credentialsDTO.Domain,
                    credentialsDTO.Username,
                    credentialsDTO.Password
                    );
            }
            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(
                ObjectFactory.GetInstance<IRestfulServiceClient>()
            );

            //verify that the post call is made 
            restClientMock.Verify(
                client => client.PostAsync<CredentialsDTO>(
                new Uri(activityTemplateDO.Terminal.Endpoint + "/authentication/token"),
                It.Is<CredentialsDTO>(it => it.Username == credentialsDTO.Username &&
                                           it.Password == credentialsDTO.Password &&
                                           it.Domain == credentialsDTO.Domain), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Exactly(1));


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
                client => client.PostAsync<ExternalAuthenticationDTO>(new Uri(terminalDO.Endpoint + "/authentication/token"),
                externalAuthenticationDTO, It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Exactly(1));

            restClientMock.VerifyAll();


        }

        [Test]
        public void CanGetOAuthInitiationURL()
        {
            var tokenDO = CreateAndAddTokenDO();

            var activityTemplateDO = new ActivityTemplateDO(
                "test_name", "test_label", "1", "test_description", tokenDO.TerminalID
            );
            activityTemplateDO.Id = FixtureData.GetTestGuidById(1);
            var activityDO = FixtureData.TestActivity1();
            activityDO.ActivityTemplate = activityTemplateDO;
            activityDO.AuthorizationToken = tokenDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.TerminalRepository.GetByKey(tokenDO.TerminalID).AuthenticationType = AuthenticationType.Internal;
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    PlanState = PlanState.Executing,
                    ChildNodes = { activityDO }
                });
                uow.SaveChanges();
            }

            TerminalDO terminal;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.GetByKey(tokenDO.UserID);
                 terminal = uow.TerminalRepository.GetByKey(tokenDO.TerminalID);
                var result = _authorization.GetOAuthInitiationURL(user, terminal);
            }
            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(ObjectFactory.GetInstance<IRestfulServiceClient>());

            //verify that the post call is made 
           
            restClientMock.Verify(
                client => client.PostAsync(
                    new Uri(terminal.Endpoint + "/authentication/request_url"), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()
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

            var activityDO = FixtureData.TestActivity1();
            var activityDTO = new ActivityDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO(
                    "test_name",
                    "test_label",
                    "1",
                    "test_description",
                    terminalDO.Id
                );

                activityTemplateDO.Id = FixtureData.GetTestGuidById(1);
                activityTemplateDO.NeedsAuthentication = true;

                uow.ActivityTemplateRepository.Add(activityTemplateDO);

                activityDO.ActivityTemplate = activityTemplateDO;
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    PlanState = PlanState.Executing,
                    ChildNodes = { activityDO }
                });

                uow.SaveChanges();

                activityDTO.Id = activityDO.Id;
                activityDTO.ActivityTemplate = Mapper.Map<ActivityTemplateSummaryDTO>(activityTemplateDO);
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testResult = _authorization.ValidateAuthenticationNeeded(uow, userDO.Id, activityDTO);

                Assert.IsTrue(testResult);
            }
        }

        [Test]
        public void ValidateAuthenticationNeededIsFalse()
        {
            var tokenDO = CreateAndAddTokenDO();
            var activityDO = FixtureData.TestActivity1();
            var activityDTO = new ActivityDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.TerminalRepository.GetByKey(tokenDO.TerminalID).AuthenticationType = AuthenticationType.Internal;
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", tokenDO.TerminalID);
                activityTemplateDO.Id = FixtureData.GetTestGuidById(1);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);

                activityDO.ActivityTemplate = activityTemplateDO;

                activityDO.AuthorizationToken = tokenDO;
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    PlanState = PlanState.Executing,
                    ChildNodes = { activityDO }
                });

                uow.SaveChanges();

                activityDTO.Id = activityDO.Id;
                activityDTO.ActivityTemplate = Mapper.Map<ActivityTemplateSummaryDTO>(activityTemplateDO);
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testResult = _authorization.ValidateAuthenticationNeeded(uow, tokenDO.UserID, activityDTO);

                Assert.IsFalse(testResult);
            }
        }

        [Test]
        public void TestAddAuthenticationCrate()
        {
            var userDO = CreateAndAddUserDO();
            var terminalDO = CreateAndAddTerminalDO();
            terminalDO.AuthenticationType = AuthenticationType.Internal;

            var activityDTO = new ActivityDTO();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplateDO = new ActivityTemplateDO("test_name", "test_label", "1", "test_description", terminalDO.Id);
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();

            }


            _authorization.AddAuthenticationCrate(activityDTO, AuthenticationType.Internal);
            Assert.IsTrue(IsCratePresents(activityDTO, AuthenticationMode.InternalMode));

            _authorization.AddAuthenticationCrate(activityDTO, AuthenticationType.External);
            Assert.IsTrue(IsCratePresents(activityDTO, AuthenticationMode.ExternalMode));

            _authorization.AddAuthenticationCrate(activityDTO, AuthenticationType.InternalWithDomain);
            Assert.IsTrue(IsCratePresents(activityDTO, AuthenticationMode.InternalModeWithDomain));
        }

        private bool IsCratePresents(ActivityDTO activityDTO, AuthenticationMode mode)
        {
            var result = false;
            foreach (var crate in activityDTO.CrateStorage.Crates)
            {
                if ((int)mode == crate.Contents["Mode"].ToObject<int>())
                {
                    result = true;
                    break;
                }

            }

            return result;
        }
    }
}
