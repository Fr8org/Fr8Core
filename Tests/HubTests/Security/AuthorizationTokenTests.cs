using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.Services;
using HubWeb.Controllers;
using Fr8.Testing.Unit;

namespace HubTests.Security
{
    // these tests are looks outdated
    [TestFixture]
    [Category("AuthToken")]
    public class AuthorizationTokenTests : BaseTest
    {
        // GetAuthorizationTokenURL has weird implementation and used only in this test. 
//        [Test]
//        public void TestAuthTokenGeneration()
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                const string originalURL = @"http://www.google.com";
//                var user = new Fr8Account();
//                var emailAddress = new EmailAddressDO
//                {
//                    Address = "rjrudman@gmail.com",
//                    Name = "Robert Rudman"
//                };
//
//                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);
//
//                var authTokenURL = uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(originalURL, userDO);
//
//                uow.SaveChanges();
//
//                //The url looks something like this: tokenAuth?token=ba8447cc-c6f7-4104-a9e9-4ddd2bb7b769
//                var token = authTokenURL.Substring("tokenAuth?token=".Length);
//
//                Assert.AreEqual(1, uow.AuthorizationTokenRepository.Count());
//                var newTokenDO = uow.AuthorizationTokenRepository.FindToken(userDO.Id, 0, null);
//                Assert.AreEqual(token, newTokenDO.Id.ToString());
//                Assert.AreEqual(userDO, newTokenDO.UserDO);
//                Assert.AreEqual(originalURL, newTokenDO.RedirectURL);
//            }
//        }

// GetAuthorizationTokenURL has weird implementation and used only in this test. 
//        [Test]
//        public void TestAuthPage()
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                const string originalURL = @"http://www.google.com";
//                var user = new Fr8Account();
//                var emailAddress = new EmailAddressDO
//                {
//                    Address = "rjrudman@gmail.com",
//                    Name = "Robert Rudman"
//                };
//                
//                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);
//
//                var authTokenURL = uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(originalURL, userDO);
//
//                uow.SaveChanges();
//
//                //The url looks something like this: tokenAuth?token=ba8447cc-c6f7-4104-a9e9-4ddd2bb7b769
//                var token = authTokenURL.Substring("tokenAuth?token=".Length);
//
//                var tokenController = new TokenAuthController();
//                
//                var res = tokenController.Index(token);
//
//                var redirectResult = res as RedirectResult;
//                Assert.NotNull(redirectResult);
//                Assert.AreEqual(originalURL, redirectResult.Url);
//
//                //Make sure we were actually logged in
//                Assert.AreEqual(userDO.Id, ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser());
//            }
//        }
    }
}
