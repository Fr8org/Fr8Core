using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hub.Managers.APIManagers.Packagers;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Hub.Services;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using Hub.StructureMap;
using Hub.Security;
using Microsoft.Owin.Security.DataProtection;
using terminalFr8Core.Interfaces;

namespace HubTests.Integration
{
    [TestFixture]
    [Category("IntegrationTests")]
    public class AccountITests : BaseTest
    {
        [Test]
        [Category("IntegrationTests")]
        public async Task ITest_CanResetPassword()
        {
            // DataProtectionProvider property is not getting initialised through startup
            // So Initiliaze it explicitly. DpapiDataProtectionProvider is used for test cases only
            DockyardIdentityManager.DataProtectionProvider = new DpapiDataProtectionProvider("fr8");
            string email;
            string id;
            // SETUP
            var account = ObjectFactory.GetInstance<Fr8Account>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = FixtureData.TestUser1();
                account.Create(uow, userDO);
                id = userDO.Id;
                email = userDO.EmailAddress.Address;
            }

            //setup IEmailPackager
            var curEmailPackager = new Mock<IEmailPackager>(MockBehavior.Default);
            ObjectFactory.Container.Inject(typeof(IEmailPackager), curEmailPackager.Object);

            // EXECUTE
            // generate a forgot password email
            await account.ForgotPasswordAsync(email);
            // get callback url from generated email
            string callbackUrl;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelopeDO = uow.EnvelopeRepository.GetQuery().Single();
                callbackUrl = (string)envelopeDO.MergeData["-callback_url-"];
            }
		    var userId = Regex.Match(callbackUrl,
		    				 "userId=(?<userId>[-a-f\\d]+)",
		    				 RegexOptions.IgnoreCase)
		        .Groups["userId"].Value;
		    var code = Regex.Match(callbackUrl,
                             "code=(?<code>[^&#]+)",
		    				 RegexOptions.IgnoreCase)
		        .Groups["code"].Value;
		    var result = await account.ResetPasswordAsync(userId, code, "123456");

            // VERIFY
            Assert.AreEqual(id, userId);
		    Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors));

            //verify whether the external email is sent
            curEmailPackager.Verify(packager => packager.Send(It.IsAny<IMailerDO>()), Times.Exactly(1));
            curEmailPackager.VerifyAll();
        }
    }
}
