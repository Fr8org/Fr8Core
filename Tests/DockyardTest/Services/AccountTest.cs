using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("AccountService")]
    public class AccountTest : BaseTest
    {
        private Account _account;
        DockyardAccountDO curDockyardAccountDO;
        private readonly string userName = "gchauhan";
        private readonly string password = "govind@123";
        LoginStatus curLogingStatus = LoginStatus.Successful;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _account = ObjectFactory.GetInstance<Account>();
            
        }

        [Test]
        public void AccountService_RegisterUser()
        {
            string firstName = "Govind";
            string lastName = "Chauhan";
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curDockyardAccountDO = _account.Register(uow, userName, firstName, lastName, password, Roles.Customer);
                Assert.IsNotNull(curDockyardAccountDO);
                Assert.AreEqual(curDockyardAccountDO.UserName, userName);
            }
        }

        [Test]
        public void AccountService_Check_User_Login()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //DockyardAccountDO curDockyardAccountDO = FixtureData.
                curDockyardAccountDO = uow.UserRepository.FindOne(x => x.UserName == "alex");
                curLogingStatus = _account.Login(uow, curDockyardAccountDO, password, true);
                Assert.AreEqual(curLogingStatus, LoginStatus.Successful);
            }
        }
    }
}
