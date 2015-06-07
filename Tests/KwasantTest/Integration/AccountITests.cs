using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Integration
{
    [TestFixture]
    public class AccountITests : BaseTest
    {
        [Test]
        [Category("IntegrationTests")]
        public async void ITest_CanResetPassword()
        {
            string email;
            string id;
            // SETUP
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixtureData = new FixtureData(uow);
                var userDO = fixtureData.TestUser1();
                uow.UserRepository.Add(userDO);
                uow.SaveChanges();
                id = userDO.Id;
                email = userDO.EmailAddress.Address;
            }

            // EXECUTE
            // generate a forgot password email
            var account = ObjectFactory.GetInstance<Account>();
            await account.ForgotPasswordAsync(email);
            // get callback url from generated email
            string callbackUrl;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelopeDO = uow.EnvelopeRepository.GetQuery().Single();
                callbackUrl = (String)envelopeDO.MergeData["-callback_url-"];
            }
            var userId = Regex.Match(callbackUrl,
                                     "userId=(?<userId>[-a-f\\d]+)",
                                     RegexOptions.IgnoreCase)
                .Groups["userId"].Value;
            var code = Regex.Match(callbackUrl,
                                     "code=(?<code>[\\d]+)",
                                     RegexOptions.IgnoreCase)
                .Groups["code"].Value;
            var result = await account.ResetPasswordAsync(userId, code, "123456");

            // VERIFY
            Assert.AreEqual(id, userId);
            Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors));
        }
    }
}
