using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static DockyardAccountDO TestUser1()
        {
            var curEmailAddressDO = FixtureData.TestEmailAddress1();
            return new DockyardAccountDO(curEmailAddressDO);
        }

        public static DockyardAccountDO TestUser2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            return new DockyardAccountDO(curEmailAddressDO);
        }

        public static DockyardAccountDO TestUser3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            return new DockyardAccountDO(curEmailAddressDO);
        }
    }
}

