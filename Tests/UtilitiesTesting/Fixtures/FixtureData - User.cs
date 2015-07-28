using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public DockyardAccountDO TestUser1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return _uow.UserRepository.GetOrCreateUser(curEmailAddressDO);
        }

        public DockyardAccountDO TestUser2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            return _uow.UserRepository.GetOrCreateUser(curEmailAddressDO);
        }

        public DockyardAccountDO TestUser3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            return _uow.UserRepository.GetOrCreateUser(curEmailAddressDO);
        }
    }
}

