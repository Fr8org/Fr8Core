using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Services;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("DockyardAccountService")]
    public class DockyardAccountTest : BaseTest
    {
        private Fr8Account _dockyardAccount;
        Fr8AccountDO _dockyardAccountDO;
        private readonly string userName = "alexlucre";
        private readonly string password = "alex@123";
        LoginStatus curLogingStatus = LoginStatus.InvalidCredential;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _dockyardAccount = ObjectFactory.GetInstance<Fr8Account>();
            _dockyardAccountDO = FixtureData.TestDockyardAccount3();
        }

        #region Test cases for method RegisterUser
        [Test]
        public void CanRegisterOrNot()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccount = _dockyardAccount.Register(uow, userName, "Alex", "Lucre", password, Roles.Admin);
                Assert.IsNotNull(curDockyardAccount);
                Assert.AreEqual(curDockyardAccount.UserName, userName);
            }
        }
        #endregion

        #region Test cases for method Login
        [Test]
        public void CanLogin()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _dockyardAccount.Login(uow, _dockyardAccountDO, password, true);
                Assert.AreEqual(curLogingStatus, LoginStatus.Successful);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsWhenDockyardAccountNull()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _dockyardAccount.Login(uow, null, password, true);
            }
        }

        [Test]
        public void FailsWhenPasswordInconrrect()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _dockyardAccount.Login(uow, _dockyardAccountDO, "abc", true);
                Assert.AreEqual(curLogingStatus, LoginStatus.InvalidCredential);
            }
        }
        #endregion

        #region Test cases for method UpdatePassword
        [Test]
        public void CanPasswordUpdateOrNot()
        {
            string passwrodHash = _dockyardAccountDO.PasswordHash;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.UpdatePassword(uow, null, "abc");
                Assert.AreEqual(passwrodHash, _dockyardAccountDO.PasswordHash);

                _dockyardAccount.UpdatePassword(uow, _dockyardAccountDO, "abc");
                Assert.AreNotEqual(passwrodHash, _dockyardAccountDO.PasswordHash);
            }
        }
        #endregion

        #region Test cases for method GetMode
        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void CanCommunicationModeDirectOrDelegetOrNull()
        {
            CommunicationMode cruCommunicationMode;
            _dockyardAccount.GetMode(null);

            cruCommunicationMode = _dockyardAccount.GetMode(_dockyardAccountDO);
            Assert.AreEqual(cruCommunicationMode, CommunicationMode.Direct);

            _dockyardAccountDO.PasswordHash = string.Empty;
            cruCommunicationMode = _dockyardAccount.GetMode(_dockyardAccountDO);
            Assert.AreEqual(cruCommunicationMode, CommunicationMode.Delegate);
        }
        #endregion

        #region Test cases for method GetDisplayName
        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsGetDisplayNameIfDockyardAccountDONull()
        {
            Fr8Account.GetDisplayName(null);
        }

        [Test]
        public void CanGetDisplayNameIfFirstNameLastName()
        {
            _dockyardAccountDO.FirstName = "Alex";
            _dockyardAccountDO.LastName = "Lucre";
            string displayName = "Alex Lucre";
            Assert.AreEqual(displayName, Fr8Account.GetDisplayName(_dockyardAccountDO));
        }

        [Test]
        public void CanGetDisplayNameIfNoLastName()
        {
            _dockyardAccountDO.FirstName = "Alex";
            _dockyardAccountDO.LastName = null;
            string displayName = "Alex";
            Assert.AreEqual(displayName, Fr8Account.GetDisplayName(_dockyardAccountDO));
        }

        [Test]
        public void CanGetDisplayNameWithEmailAddressDO()
        {
            _dockyardAccountDO.EmailAddress = FixtureData.TestEmailAddress1();
            string displayName = "Alex";
            Assert.AreEqual(displayName, Fr8Account.GetDisplayName(_dockyardAccountDO));
        }

        [Test]
        public void CanGetDisplayNAmeIfEmailAddressDONameEmpty()
        {
            _dockyardAccountDO.EmailAddress = FixtureData.TestEmailAddress1();
            _dockyardAccountDO.EmailAddress.Name = null;
            string displayName = "alexlucre1";
            Assert.AreEqual(displayName, Fr8Account.GetDisplayName(_dockyardAccountDO));
        }
        #endregion

        #region Test cases for method Create
        [Test]
        public void CanCreatedUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                Fr8AccountDO curDockyardAccountLocalNew = uow.UserRepository.GetQuery().Where(u => u.UserName == curDockyardAccountLocal.UserName).FirstOrDefault();
                Assert.AreEqual(curDockyardAccountLocalNew.UserName, curDockyardAccountLocal.UserName);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsCreateUserIfNoDockyardAccountDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.Create(uow, null);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsCreateUserIfNoUnitOfWork()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.Create(null, _dockyardAccountDO);
            }
        }
        #endregion

        #region Test cases for method GetExisting
        [Test]
        public void CanGetExistingUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                Assert.NotNull(_dockyardAccount.GetExisting(uow, curDockyardAccountLocal.EmailAddress.Address));
            }
        }

        [Test]
        public void FailsGetExistingUserIfNoEmailAddress()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                Assert.Null(_dockyardAccount.GetExisting(uow, ""));
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsGetExistingUserIfNoUnitOfWork()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.GetExisting(null, "alexlucre1@gmail.com");
            }
        }
        #endregion

        #region Test cases for method Update
        [Test]
        public void CanUpdateUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                _dockyardAccount.Update(uow, curDockyardAccountLocal, _dockyardAccountDO);
                Fr8AccountDO curDockyardAccountLocalNew = uow.UserRepository.GetQuery().Where(u => u.UserName == curDockyardAccountLocal.UserName).FirstOrDefault();
                Assert.AreEqual(curDockyardAccountLocal, curDockyardAccountLocalNew);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsUserUpdatedIfNoDockyardAccountToSubmit()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                 _dockyardAccount.Update(uow, null, _dockyardAccountDO);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsUserUpdatedIfNoExistingDockyardAccount()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.Update(uow, FixtureData.TestDockyardAccount4(), null);
            }
        }
        #endregion
    }
}
