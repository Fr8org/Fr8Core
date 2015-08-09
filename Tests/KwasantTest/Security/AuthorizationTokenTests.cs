using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantWeb.Controllers;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Security
{
    [TestFixture]
    public class AuthorizationTokenTests : BaseTest
    {
        [Test]
        public void TestAuthTokenGeneration()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                const string originalURL = @"http://www.google.com";
                var user = new User();
                var emailAddress = new EmailAddressDO
                {
                    Address = "rjrudman@gmail.com",
                    Name = "Robert Rudman"
                };

                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);

                var authTokenURL = uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(originalURL, userDO);

                uow.SaveChanges();

                //The url looks something like this: tokenAuth?token=ba8447cc-c6f7-4104-a9e9-4ddd2bb7b769
                var token = authTokenURL.Substring("tokenAuth?token=".Length);

                Assert.AreEqual(1, uow.AuthorizationTokenRepository.GetQuery().Count());
                var newTokenDO = uow.AuthorizationTokenRepository.GetQuery().First();
                Assert.AreEqual(token, newTokenDO.Id.ToString());
                Assert.AreEqual(userDO, newTokenDO.UserDO);
                Assert.AreEqual(originalURL, newTokenDO.RedirectURL);
            }
        }

        [Test]
        public void TestAuthPage()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                const string originalURL = @"http://www.google.com";
                var user = new User();
                var emailAddress = new EmailAddressDO
                {
                    Address = "rjrudman@gmail.com",
                    Name = "Robert Rudman"
                };

                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);

                var authTokenURL = uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(originalURL, userDO);

                uow.SaveChanges();

                //The url looks something like this: tokenAuth?token=ba8447cc-c6f7-4104-a9e9-4ddd2bb7b769
                var token = authTokenURL.Substring("tokenAuth?token=".Length);

                var tokenController = new TokenAuthController();
                
                var res = tokenController.Index(token);

                var redirectResult = res as RedirectResult;
                Assert.NotNull(redirectResult);
                Assert.AreEqual(originalURL, redirectResult.Url);

                //Make sure we were actually logged in
                Assert.AreEqual(userDO.Id, ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser());
            }
        }
    }
}
