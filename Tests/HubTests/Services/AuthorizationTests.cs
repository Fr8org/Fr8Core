using System;
using System.Linq;
using Hub.StructureMap;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit.Fixtures;
using System.Threading.Tasks;
using Moq;
using Hub.Services;
using System.Collections.Generic;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Testing.Unit;

namespace HubTests.Services
{
    [TestFixture]
    [Category("CrateManager")]
    public partial class AuthorizationTests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test]
        public async Task AuthenticateInternal_Multiplefr8Accounts_GenerateAuthorizationTokenDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                var currentAccount = FixtureData.TestDockyardAccount1();
                uow.UserRepository.Add(currentAccount);

                var developerAccount = FixtureData.TestDeveloperAccount();
                uow.UserRepository.Add(developerAccount);

                var terminal = FixtureData.TerminalSix();
                uow.TerminalRepository.Add(terminal);

                uow.SaveChanges();

                var credentialsDTO = new CredentialsDTO()
                {
                    Username = "Username",
                    Password = "Password",
                    Domain = "Domain"
                };

                var authorizationToken = FixtureData.AuthorizationTokenTest1();

                var response = JsonConvert.SerializeObject(authorizationToken);
                var restfulServiceClient = new Mock<IRestfulServiceClient>();
                restfulServiceClient.Setup(r => r.PostAsync<CredentialsDTO>(new Uri(terminal.Endpoint + "/authentication/token"),
                        It.Is<CredentialsDTO>(it => it.Username == credentialsDTO.Username
                            && it.Password == credentialsDTO.Password
                            && it.Domain == credentialsDTO.Domain), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                     .Returns(Task.FromResult(response));
                ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));

                var authorization = ObjectFactory.GetInstance<Authorization>();

                //Act 
                await authorization.AuthenticateInternal(currentAccount, terminal, credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
                await authorization.AuthenticateInternal(developerAccount, terminal, credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);

                //Assert
                int actualResult = uow.AuthorizationTokenRepository.GetPublicDataQuery().Count(x => x.ExternalAccountId == authorizationToken.ExternalAccountId);
                Assert.AreEqual(2, actualResult);
            }
        }


    }
}


