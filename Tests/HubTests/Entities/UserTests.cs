using System.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Services;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Entities
{
    [TestFixture]
    public class UserTests : BaseTest
    {
        [Test, ExpectedException(ExpectedMessage = "Duplicate values for 'EmailAddressID' on 'Fr8AccountDO' are not allowed. Duplicated value: '1'")]
        public void TestDuplicateUserEmailIDRejected()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EmailAddressRepository.Add(new EmailAddressDO() {Id = 1});
                uow.UserRepository.Add(new Fr8AccountDO() { EmailAddressID = 1, State = UserState.Active });
                uow.UserRepository.Add(new Fr8AccountDO() { EmailAddressID = 1, State = UserState.Active });
                uow.SaveChanges();
            }
        }

        [Test]
        [Category("User")]
        public void CanAddUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                uow.AspNetRolesRepository.Add(FixtureData.TestRole());

                //SETUP                 

                Fr8AccountDO currDockyardAccountDO = new Fr8AccountDO();
                uow.UserRepository.Add(currDockyardAccountDO);
               
                Fr8AccountDO currRetrivedDockyardAccountDO = uow.UserRepository.GetQuery().FirstOrDefault(uu => currDockyardAccountDO.EmailAddressID == uu.EmailAddressID);
            }
        }
    }
}
