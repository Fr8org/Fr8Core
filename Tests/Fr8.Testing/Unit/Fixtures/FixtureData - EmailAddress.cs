using Data.Entities;
using Data.Interfaces;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
        public FixtureData(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private IUnitOfWork _uow;

        public static EmailAddressDO TestEmailAddress1()
        {
            var emailAddressDO = new EmailAddressDO("alexlucre1@gmail.com");
            emailAddressDO.Id = 1;
            emailAddressDO.Name = "Alex";
            return emailAddressDO;
        }

        public static EmailAddressDO TestEmailAddress2()
        {
            var emailAddressDO = new EmailAddressDO("joetest2@edelstein.org");
            emailAddressDO.Id = 2;
            return emailAddressDO;
        }
        public static EmailAddressDO TestEmailAddress3()
        {
            var emailAddressDO = new EmailAddressDO("integrationtesting@kwasant.net");
            emailAddressDO.Id = 3;
            return emailAddressDO;
        }

        public static EmailAddressDO TestEmailAddress4()
        {
            var emailAddressDO = new EmailAddressDO("JackMaginot@gmail.com");
            emailAddressDO.Id = 4;
            return emailAddressDO;
        }

        public static EmailAddressDO TestEmailAddress5()
        {
            var emailAddressDO = new EmailAddressDO("RobMaginot@gmail.com");
            emailAddressDO.Id = 5;
            return emailAddressDO;
        }

        public static EmailAddressDO TestEmailAddress6()
        {
            var emailAddressDO = new EmailAddressDO("nilesh@gmail.com");
            emailAddressDO.Id = 6;
            return emailAddressDO;
        }

        public static EmailAddressDO TestEmailAddress7()
        {
            var emailAddressDO = new EmailAddressDO("system1@fr8.co");
            emailAddressDO.Id = 7;
            return emailAddressDO;
        }

        public static EmailAddressDO TestEmailAddress8()
        {
            var emailAddressDO = new EmailAddressDO("testuser@fr8.co");
            emailAddressDO.Id = 8;
            return emailAddressDO;
        }
    }
}
