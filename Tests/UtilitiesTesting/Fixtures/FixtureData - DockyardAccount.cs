using Data.Entities;

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

