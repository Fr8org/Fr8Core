﻿using System.Linq;
using Core.Services;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Integration
{
    [TestFixture]
    public class AccountITests : BaseTest
    {
        [Test]
        [Category("IntegrationTests")]
        [Ignore]
        public async void ITest_CanResetPassword()
        {
            string email;
            string id;
            // SETUP
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
               
                var userDO = FixtureData.TestUser1();
                uow.UserRepository.Add(userDO);
                uow.SaveChanges();
                id = userDO.Id;
                email = userDO.EmailAddress.Address;
            }

            // EXECUTE
            // generate a forgot password email
            var account = ObjectFactory.GetInstance<DockyardAccount>();
            await account.ForgotPasswordAsync(email);
            // get callback url from generated email
            string callbackUrl;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelopeDO = uow.EnvelopeRepository.GetQuery().Single();
                //callbackUrl = (String)envelopeDO.MergeData["-callback_url-"];
            }
		//var userId = Regex.Match(callbackUrl,
		//				 "userId=(?<userId>[-a-f\\d]+)",
		//				 RegexOptions.IgnoreCase)
		//    .Groups["userId"].Value;
		//var code = Regex.Match(callbackUrl,
		//				 "code=(?<code>[\\d]+)",
		//				 RegexOptions.IgnoreCase)
		//    .Groups["code"].Value;
		//var result = await account.ResetPasswordAsync(userId, code, "123456");

            // VERIFY
		//Assert.AreEqual(id, userId);
		//Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors));
        }
    }
}
