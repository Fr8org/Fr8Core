using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Services;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static Fr8AccountDO TestDockyardAccount1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return new Fr8AccountDO()
            {
                Id = "testuser1",
                EmailAddress = curEmailAddressDO,
                FirstName = "Alex",
                State = 1
            };
        }
        public static Fr8AccountDO TestDockyardAccount2()
        {
            var curEmailAddressDO = TestEmailAddress2();
            return new Fr8AccountDO()
            {
                Id = "testUser1",
                EmailAddress = curEmailAddressDO,
                FirstName = "Alex",
                State = 1
            };
        }

        public static Fr8AccountDO TestDeveloperAccount()
        {
            var curEmailAddressDO = TestEmailAddress2();
            return new Fr8AccountDO()
            {
                Id = "developerfoo",
                EmailAddress = curEmailAddressDO,
                FirstName = "developer",
                State = 1
            };
        }

        public static Fr8AccountDO TestDockyardAccount3()
        {
            Fr8Account _dockyardAccount = ObjectFactory.GetInstance<Fr8Account>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return _dockyardAccount.Register(uow, "alexlucre", "Alex", "Lucre1", "alex@123", Roles.Admin);
            }
        }

        public static Fr8AccountDO TestDockyardAccount4()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return new Fr8AccountDO()
            {
                EmailAddress = curEmailAddressDO,
                FirstName = "Alex",
                LastName = "Lucre1",
                UserName = "alexlucre"
               
            };


        }

        public static Fr8AccountDO TestDockyardAccount5()
        {
            var curEmailAddressDO = TestEmailAddress6();
            return new Fr8AccountDO()
            {
                EmailAddress = curEmailAddressDO,
                Id = "testuser",
                FirstName = "GMCS",
                LastName = "Team",
                UserName = "testing",
                State = 1

            };
        }

        public static Fr8AccountDO TestDockyardAccount6()
        {
            string guestUserEmail = "guestuser@test.com";
            string password = "oldpassword";
            string firstName = " Guest";
            string lastName = " User";

            Fr8Account _dockyardAccount = ObjectFactory.GetInstance<Fr8Account>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var guestUserFr8Account = _dockyardAccount.Register(uow, guestUserEmail, firstName, lastName, password, Roles.Guest);
                uow.AspNetUserRolesRepository.RevokeRoleFromUser(Roles.Customer,guestUserFr8Account.Id);
                uow.SaveChanges();

                return guestUserFr8Account;
            }
        }

        /* public DockyardAccountDO TestDockyardAccount2()
         {
             var curEmailAddressDO = TestEmailAddress5();
             return _uow.DockyardAccountRepository.GetOrCreateDockyardAccount(curEmailAddressDO);
         }

         public DockyardAccountDO TestDockyardAccount3()
         {
             var curEmailAddressDO = TestEmailAddress3();
             return _uow.DockyardAccountRepository.GetOrCreateDockyardAccount(curEmailAddressDO);
         }*/
    }
}

