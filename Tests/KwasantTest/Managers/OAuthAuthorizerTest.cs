using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Managers
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
                uow.RemoteCalendarProviderRepository.CreateRemoteCalendarProviders(configRepository);
                uow.SaveChanges();
            }
        }

        [Test]
        [Ignore]
        public void CanOAuthRedirectToCallbackUrl()
        {
            // SETUP
            UserDO user;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixtureData = new FixtureData(uow);
                user = fixtureData.TestUser1();
                uow.SaveChanges();
            }

            // EXECUTE
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var oauthProviders = uow.RemoteCalendarProviderRepository.GetQuery().Where(p => p.AuthType == ServiceAuthorizationType.OAuth2).ToArray();
                Assert.That(oauthProviders.Length > 0, "No OAuth providers.");
                foreach (var provider in oauthProviders)
                {
                    var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(provider.Name);
                    var result = authorizer.AuthorizeAsync(
                        user.Id,
                        user.EmailAddress.Address,
                        UserController.GetCallbackUrl(provider.Name, "https://www.kwasant.com/"),
                        "https://www.kwasant.com/",
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
