using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Core.Services;
using Core.StructureMap;
using DockyardTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class UserTests : BaseTest
    {
        [Test, ExpectedException(ExpectedMessage = "Duplicate values for 'EmailAddressID' on 'UserDO' are not allowed. Duplicated value: '1'")]
        public void TestDuplicateUserEmailIDRejected()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EmailAddressRepository.Add(new EmailAddressDO() {Id = 1});
                uow.UserRepository.Add(new UserDO() { EmailAddressID = 1, State = UserState.Active });
                uow.UserRepository.Add(new UserDO() { EmailAddressID = 1, State = UserState.Active });
                uow.SaveChanges();
            }
        }

        [Test]
        [Category("User")]
        public void CanAddUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                uow.AspNetRolesRepository.Add(fixture.TestRole());
                var u = new UserDO();
                var user = new User();

                //SETUP                 

                UserDO currUserDO = new UserDO();
                uow.UserRepository.Add(currUserDO);
               
                UserDO currRetrivedUserDO = uow.UserRepository.GetQuery().FirstOrDefault(uu => currUserDO.EmailAddressID == uu.EmailAddressID);
            }
        }
    }
}
