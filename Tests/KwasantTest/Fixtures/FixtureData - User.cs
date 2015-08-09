using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public UserDO TestUser1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return _uow.UserRepository.GetOrCreateUser(curEmailAddressDO);
        }

        public UserDO TestUser2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            return _uow.UserRepository.GetOrCreateUser(curEmailAddressDO);
        }

        public UserDO TestUser3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            return _uow.UserRepository.GetOrCreateUser(curEmailAddressDO);
        }
    }
}

