using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Microsoft.AspNet.Identity.EntityFramework;
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
    [Category("DockyardAccountService")]
    public class DockyardAccountTest : BaseTest
    {
        private DockyardAccount _dockyardAccount;
        DockyardAccountDO curDockyardAccount;
        private readonly string userName = "gchauhan";
        private readonly string password = "govind@123";
        LoginStatus curLogingStatus = LoginStatus.Successful;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _dockyardAccount = ObjectFactory.GetInstance<DockyardAccount>();
            curDockyardAccount = FixtureData.TestDockyardAccount3();
        }

        #region Test cases for method RegisterUser
        [Test]
        public void RegisterUser_CheckRegisteredOrNot()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccount = _dockyardAccount.Register(uow, userName, "Govind", "Chauhan", password, Roles.Customer);
                Assert.IsNotNull(curDockyardAccount);
                Assert.AreEqual(curDockyardAccount.UserName, userName);
            }
        }
        #endregion

        #region Test cases for method Login
        [Test]
        public void Login_CheckLoginSucess()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _dockyardAccount.Login(uow, curDockyardAccount, password, true);
                Assert.AreEqual(curLogingStatus, LoginStatus.Successful);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void Login_CheckLoginFailureWithNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _dockyardAccount.Login(uow, null, password, true);
            }
        }

        [Test]
        public void Login_CheckLoginPasswordMismatch()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curLogingStatus = _dockyardAccount.Login(uow, curDockyardAccount, "abc", true);
                Assert.AreEqual(curLogingStatus, LoginStatus.InvalidCredential);
            }
        }
        #endregion

        #region Test cases for method UpdatePassword
        [Test]
        public void UpdatePassword_CheckPasswordUpdatedOrNot()
        {
            string passwrodHash = curDockyardAccount.PasswordHash;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.UpdatePassword(uow, null, "abc");
                Assert.AreEqual(passwrodHash, curDockyardAccount.PasswordHash);

                _dockyardAccount.UpdatePassword(uow, curDockyardAccount, "abc");
                Assert.AreNotEqual(passwrodHash, curDockyardAccount.PasswordHash);

            }
        }
        #endregion

        #region Test cases for method GetMode
        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void GetMode_CheckCommunicationModeDirectOrDelegetOrNullRef()
        {
            CommunicationMode cruCommunicationMode;
            _dockyardAccount.GetMode(null);

            cruCommunicationMode = _dockyardAccount.GetMode(curDockyardAccount);
            Assert.AreEqual(cruCommunicationMode, CommunicationMode.Direct);

            curDockyardAccount.PasswordHash = string.Empty;
            cruCommunicationMode = _dockyardAccount.GetMode(curDockyardAccount);
            Assert.AreEqual(cruCommunicationMode, CommunicationMode.Delegate);
        }
        #endregion

        #region Test cases for method GetDisplayName
        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void GetDisplayName_CheckDisplayNameWithDockyardAccountDONull()
        {
            DockyardAccount.GetDisplayName(null);
        }

        [Test]
        public void GetDisplayName_CheckDisplayNameWithFirstNameLastName()
        {
            curDockyardAccount.FirstName = "Govind";
            curDockyardAccount.LastName = "Chauhan";
            string displayName = "Govind Chauhan";
            Assert.AreEqual(displayName, DockyardAccount.GetDisplayName(curDockyardAccount));
        }

        [Test]
        public void GetDisplayName_CheckDisplayWithFirstName()
        {
            curDockyardAccount.FirstName = "Govind";
            curDockyardAccount.LastName = null;
            string displayName = "Govind";
            Assert.AreEqual(displayName, DockyardAccount.GetDisplayName(curDockyardAccount));
        }

        [Test]
        public void GetDisplayName_CheckDisplayWithEmailAddressDO()
        {
            curDockyardAccount.EmailAddress = FixtureData.TestEmailAddress6();
            string displayName = "Govind";
            Assert.AreEqual(displayName, DockyardAccount.GetDisplayName(curDockyardAccount));
        }

        [Test]
        public void GetDisplayName_CheckDisplayWithEmailAddressDONameEmpty()
        {
            curDockyardAccount.EmailAddress = FixtureData.TestEmailAddress6();
            curDockyardAccount.EmailAddress.Name = null;
            string displayName = "chauhangovind3";
            Assert.AreEqual(displayName, DockyardAccount.GetDisplayName(curDockyardAccount));
        }
        #endregion

        #region Test cases for method Create
        [Test]
        public void Create_CheckUserCreatedOrNot()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                DockyardAccountDO curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                DockyardAccountDO curDockyardAccountLocalNew = uow.UserRepository.GetQuery().Where(u => u.UserName == curDockyardAccountLocal.UserName).FirstOrDefault();
                Assert.AreEqual(curDockyardAccountLocalNew.UserName, curDockyardAccountLocal.UserName);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void Create_CheckUserCreatedOrNotWithNullDockyardAccountDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.Create(uow, null);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void Create_CheckUserCreatedOrNotWithNullUnitOfWork()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.Create(null, curDockyardAccount);
            }
        }
        #endregion

        #region Test cases for method GetExisting
        [Test]
        public void GetExisting_CanGetExistingUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                Assert.NotNull(_dockyardAccount.GetExisting(uow, curDockyardAccountLocal.EmailAddress.Address));
            }
        }

        [Test]
        public void GetExisting_CanGetExistingUserWithEmptyOrNull()
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
        public void GetExisting_CanGetExistingUserWithNullUOW()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.GetExisting(null, "chauhangovind3@gmail.com");
            }
        }
        #endregion

        #region Test cases for method Update
        [Test]
        public void Update_CheckUserUpdatedOrNot()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
                _dockyardAccount.Update(uow, curDockyardAccountLocal, curDockyardAccount);
                DockyardAccountDO curDockyardAccountLocalNew = uow.UserRepository.GetQuery().Where(u => u.UserName == curDockyardAccountLocal.UserName).FirstOrDefault();
                Assert.AreEqual(curDockyardAccountLocal, curDockyardAccountLocalNew);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void Update_CheckUserUpdatedOrNotWithNullSubmittedAccount()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                 _dockyardAccount.Update(uow, null, curDockyardAccount);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void Update_CheckUserUpdatedOrNotWithNullExistingAccount()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _dockyardAccount.Update(uow, FixtureData.TestDockyardAccount4(), null);
            }
        }
        #endregion
    }
}
