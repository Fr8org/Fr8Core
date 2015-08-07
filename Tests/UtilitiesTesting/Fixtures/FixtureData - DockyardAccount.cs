using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public DockyardAccountDO TestDockyardAccount1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return _uow.DockyardAccountRepository.GetOrCreateDockyardAccount(curEmailAddressDO);
        }

        public DockyardAccountDO TestDockyardAccount2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            return _uow.DockyardAccountRepository.GetOrCreateDockyardAccount(curEmailAddressDO);
        }

        public DockyardAccountDO TestDockyardAccount3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            return _uow.DockyardAccountRepository.GetOrCreateDockyardAccount(curEmailAddressDO);
        }
    }
}

