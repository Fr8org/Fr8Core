using Data.Entities;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static Fr8AccountDO TestUser1()
        {
            var curEmailAddressDO = FixtureData.TestEmailAddress1();
            return new Fr8AccountDO(curEmailAddressDO);
        }

        public static Fr8AccountDO TestUser2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            return new Fr8AccountDO(curEmailAddressDO);
        }

        public static Fr8AccountDO TestUser3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            return new Fr8AccountDO(curEmailAddressDO);
        }
    }
}

