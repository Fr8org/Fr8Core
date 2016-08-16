using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Services;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using Hub.Security;
using Microsoft.Owin.Security.DataProtection;

namespace HubTests.Services
{
    [TestFixture]
    [Category("AccountService")]
    public class AccountTest : BaseTest
    {
        private Fr8Account _fr8Account;
        Fr8AccountDO _dockyardAccountDO;
        private readonly string userName = "alexlucre";
        private readonly string password = "alex@123";
        LoginStatus curLogingStatus = LoginStatus.InvalidCredential;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _fr8Account = ObjectFactory.GetInstance<Fr8Account>();
            _dockyardAccountDO = FixtureData.TestDockyardAccount3();
        }

        #region Test cases for method RegisterUser
        [Test]
        public void CanRegisterOrNot()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccount = _fr8Account.Register(uow, userName, "Alex", "Lucre", password, Roles.Admin);
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
                curLogingStatus = _fr8Account.Login(uow, _dockyardAccountDO, password, true);
                Assert.AreEqual(curLogingStatus, LoginStatus.Successful);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsWhenDockyardAccountNull()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _fr8Account.Login(uow, null, password, true);
            }
        }

        [Test]
        public void FailsWhenPasswordInconrrect()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _fr8Account.Login(uow, _dockyardAccountDO, "abc", true);
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
                _fr8Account.UpdatePassword(uow, null, "abc");
                Assert.AreEqual(passwrodHash, _dockyardAccountDO.PasswordHash);

                _fr8Account.UpdatePassword(uow, _dockyardAccountDO, "abc");
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
            _fr8Account.GetMode(null);

            cruCommunicationMode = _fr8Account.GetMode(_dockyardAccountDO);
            Assert.AreEqual(cruCommunicationMode, CommunicationMode.Direct);

            _dockyardAccountDO.PasswordHash = string.Empty;
            cruCommunicationMode = _fr8Account.GetMode(_dockyardAccountDO);
            Assert.AreEqual(cruCommunicationMode, CommunicationMode.Delegate);
        }
        #endregion

        #region Test cases for method GetDisplayName
        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsGetDisplayNameIfDockyardAccountDONull()
        {
            _fr8Account.GetDisplayName(null);
        }

        [Test]
        public void CanGetDisplayNameIfFirstNameLastName()
        {
            _dockyardAccountDO.FirstName = "Alex";
            _dockyardAccountDO.LastName = "Lucre";
            string displayName = "Alex Lucre";
            Assert.AreEqual(displayName, _fr8Account.GetDisplayName(_dockyardAccountDO));
        }

        [Test]
        public void CanGetDisplayNameIfNoLastName()
        {
            _dockyardAccountDO.FirstName = "Alex";
            _dockyardAccountDO.LastName = null;
            string displayName = "Alex";
            Assert.AreEqual(displayName, _fr8Account.GetDisplayName(_dockyardAccountDO));
        }

        [Test]
        public void CanGetDisplayNameWithEmailAddressDO()
        {
            _dockyardAccountDO.EmailAddress = FixtureData.TestEmailAddress1();
            string displayName = "Alex";
            Assert.AreEqual(displayName, _fr8Account.GetDisplayName(_dockyardAccountDO));
        }

        [Test]
        public void CanGetDisplayNAmeIfEmailAddressDONameEmpty()
        {
            _dockyardAccountDO.EmailAddress = FixtureData.TestEmailAddress1();
            _dockyardAccountDO.EmailAddress.Name = null;
            string displayName = "alexlucre1";
            Assert.AreEqual(displayName, _fr8Account.GetDisplayName(_dockyardAccountDO));
        }
        #endregion

        #region Test cases for method Create
        [Test]
        public void CanCreatedUser()
        {
            // DataProtectionProvider property is not getting initialised through startup
            // So Initiliaze it explicitly. DpapiDataProtectionProvider is used for test cases only
            DockyardIdentityManager.DataProtectionProvider = new DpapiDataProtectionProvider("fr8");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _fr8Account.Create(uow, curDockyardAccountLocal);
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
                _fr8Account.Create(uow, null);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsCreateUserIfNoUnitOfWork()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _fr8Account.Create(null, _dockyardAccountDO);
            }
        }
        #endregion

        #region Test cases for method GetExisting
        [Test]
        public void CanGetExistingUser()
        {
            // DataProtectionProvider property is not getting initialised through startup
            // So Initiliaze it explicitly. DpapiDataProtectionProvider is used for test cases only
            DockyardIdentityManager.DataProtectionProvider = new DpapiDataProtectionProvider("fr8");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _fr8Account.Create(uow, curDockyardAccountLocal);
                Assert.NotNull(_fr8Account.GetExisting(uow, curDockyardAccountLocal.EmailAddress.Address));
            }
        }

        [Test]
        public void FailsGetExistingUserIfNoEmailAddress()
        {
            // DataProtectionProvider property is not getting initialised through startup
            // So Initiliaze it explicitly. DpapiDataProtectionProvider is used for test cases only
            DockyardIdentityManager.DataProtectionProvider = new DpapiDataProtectionProvider("fr8");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _fr8Account.Create(uow, curDockyardAccountLocal);
                Assert.Null(_fr8Account.GetExisting(uow, ""));
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsGetExistingUserIfNoUnitOfWork()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _fr8Account.GetExisting(null, CloudConfigurationManager.GetSetting("TestUserAccountName"));
            }
        }
        #endregion

        #region Test cases for method Update
        [Test]
        public void CanUpdateUser()
        {
            // DataProtectionProvider property is not getting initialised through startup
            // So Initiliaze it explicitly. DpapiDataProtectionProvider is used for test cases only
            DockyardIdentityManager.DataProtectionProvider = new DpapiDataProtectionProvider("fr8");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _fr8Account.Create(uow, curDockyardAccountLocal);
                _fr8Account.Update(uow, curDockyardAccountLocal, _dockyardAccountDO);
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
                 _fr8Account.Update(uow, null, _dockyardAccountDO);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void FailsUserUpdatedIfNoExistingDockyardAccount()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _fr8Account.Update(uow, FixtureData.TestDockyardAccount4(), null);
            }
        }
        #endregion

        #region Test cases for Guest User Mode Operations
        
        [Test]
        public async Task CanAddAuthenticateGuestUser()
        {
            Tuple<LoginStatus,string> resultTuple   = await _fr8Account.CreateAuthenticateGuestUser();
            Assert.AreEqual(resultTuple.Item1, LoginStatus.Successful);
        }

        [Test]
        public async Task CanRegisterAndUpdateGuestUser()
        {
            Fr8AccountDO guestUserAccount = FixtureData.TestDockyardAccount6();
            string newEmail="fr8user@test.com";
            string newPassword = "newpassword";
            RegistrationStatus registrationStatus;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                registrationStatus = await _fr8Account.UpdateGuestUserRegistration(uow, newEmail, newPassword, guestUserAccount.EmailAddress.Address);
            }

            // Assert
            Assert.AreEqual(registrationStatus, RegistrationStatus.Successful);
            Assert.AreEqual(guestUserAccount.EmailAddress.Address, newEmail);
        }
        #endregion
    }
}
