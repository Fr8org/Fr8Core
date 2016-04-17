using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Managers.APIManagers.Authorizers;
using HubWeb.Controllers;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace HubTests.Managers
{
    [TestFixture]
    public class OAuthAuthorizerTest : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            ObjectFactory.Configure(cfg => cfg.For<IConfigRepository>().Use<ConfigRepository>());

            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RemoteServiceProviderRepository.CreateRemoteServiceProviders(configRepository);
                uow.SaveChanges();
            }
        }

        [Test]
        [Ignore]
        public void CanOAuthRedirectToCallbackUrl()
        {
            // SETUP
            Fr8AccountDO dockyardAccount;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
               
                dockyardAccount = FixtureData.TestUser1();
                uow.SaveChanges();
            }

            // EXECUTE
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var oauthProviders = uow.RemoteServiceProviderRepository.GetQuery()
                    .Where(p => p.AuthType == ServiceAuthorizationType.OAuth2).ToArray();

                Assert.That(oauthProviders.Length > 0, "No OAuth providers.");
                foreach (var provider in oauthProviders)
                {
                    var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(provider.Name);
                    var result = authorizer.AuthorizeAsync(
                        dockyardAccount.Id,
                        dockyardAccount.EmailAddress.Address,
                        UserController.GetCallbackUrl(provider.Name, "https://www.dockyard.company/"),
                        "https://www.dockyard.company/",
                        CancellationToken.None).Result;

                    // VERIFY
                    Assert.IsFalse(result.IsAuthorized, "User should be unauthorized.");
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = httpClient.GetAsync(result.RedirectUri).Result)
                        {
                            // now here we are getting "403 Unathorized" instead of expected "200 OK" or "400 Bad Request", have to deal with google authorization somehow
                            Assert.IsTrue(response.IsSuccessStatusCode, string.Format("Redirected URL returned: {0}", response.StatusCode));
                        }
                    }
                }
            }
        }
    }
}
