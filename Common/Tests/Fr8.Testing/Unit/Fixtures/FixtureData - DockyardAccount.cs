using System;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Services;

namespace Fr8.Testing.Unit.Fixtures
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

        public static Fr8AccountDO TestAdminAccount()
        {
            var curEmailAddressDO = TestEmailAddress7();

            return new Fr8AccountDO()
            {
                Id = "testuser",
                EmailAddress = curEmailAddressDO,
                FirstName = "admin",
                State = 1,
                Email = "system1@fr.co"
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
                uow.AspNetUserRolesRepository.RevokeRoleFromUser(Roles.StandardUser, guestUserFr8Account.Id);
                uow.SaveChanges();

                return guestUserFr8Account;
            }
        }
        public static Fr8AccountDO TestDockyardAccount7()
        {
            string adminUserEmail = "admin@test.com";
            string password = "oldpassword";
            string firstName = "Admin";
            string lastName = "Admin";

            Fr8Account _dockyardAccount = ObjectFactory.GetInstance<Fr8Account>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var adminRoleId = Guid.NewGuid().ToString();
                var adminRoleDO = new AspNetRolesDO()
                {
                    Name = "Admin",
                    Id = adminRoleId,
                    CreateDate = DateTimeOffset.UtcNow,
                    LastUpdated = DateTimeOffset.UtcNow,
                };
                uow.AspNetRolesRepository.Add(adminRoleDO);
                var adminUserFr8Account = _dockyardAccount.Register(uow, adminUserEmail, firstName, lastName, password, adminRoleId);
                uow.AspNetUserRolesRepository.RevokeRoleFromUser(Roles.Admin, adminUserFr8Account.Id);
                var adminRole = new IdentityUserRole() { RoleId = adminRoleId, UserId = adminUserFr8Account.Id };
                adminUserFr8Account.Roles.Add(adminRole);
                uow.SaveChanges();
                return adminUserFr8Account;
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

