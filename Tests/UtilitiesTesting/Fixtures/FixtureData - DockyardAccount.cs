using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static DockyardAccountDO TestDockyardAccount1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return new DockyardAccountDO()
            {
                Id = "testuser1",
                EmailAddress = curEmailAddressDO,
                FirstName = "Alex",
                State = 1
            };
        }
        public static DockyardAccountDO TestDockyardAccount2()
        {
            var curEmailAddressDO = TestEmailAddress2();
            return new DockyardAccountDO()
            {
                Id = "testUser1",
                EmailAddress = curEmailAddressDO,
                FirstName = "Alex",
                State = 1
            };
        }

        public static DockyardAccountDO TestDeveloperAccount()
        {
            var curEmailAddressDO = TestEmailAddress2();
            return new DockyardAccountDO()
            {
                Id = "developerfoo",
                EmailAddress = curEmailAddressDO,
                FirstName = "developer",
                State = 1
            };
        }

        public static DockyardAccountDO TestDockyardAccount3()
        {
            DockyardAccount _dockyardAccount = ObjectFactory.GetInstance<DockyardAccount>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return _dockyardAccount.Register(uow, "chauhangovind3@gmail.com", "Govind", "Chauhan", "govind@123", Roles.Admin);
            }
           /* DockyardAccount _dockyardAccount = ObjectFactory.GetInstance<DockyardAccount>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                DockyardAccountDO curDockyardAccountLocal = FixtureData.TestDockyardAccount4();
                _dockyardAccount.Create(uow, curDockyardAccountLocal);
               return uow.UserRepository.GetQuery().Where(u => u.UserName == curDockyardAccountLocal.UserName).FirstOrDefault();
            }*/
        }

        public static DockyardAccountDO TestDockyardAccount4()
        {
            var curEmailAddressDO = TestEmailAddress6();
            return new DockyardAccountDO()
            {
                EmailAddress = curEmailAddressDO,
                FirstName = "Govind",
                LastName = "Chauhan",
                UserName = "gchauhan"
               
            };


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

