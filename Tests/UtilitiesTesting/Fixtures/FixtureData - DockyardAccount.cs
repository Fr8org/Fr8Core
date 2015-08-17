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
                Id = "testUser1",
                EmailAddress = curEmailAddressDO,
                FirstName = "Alex"
            };
            //return _uow.DockyardAccountRepository.GetOrCreateDockyardAccount(curEmailAddressDO);
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

