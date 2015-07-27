using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Core.Services;
using Core.StructureMap;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class UserTests : BaseTest
    {
        [Test, ExpectedException(ExpectedMessage = "Duplicate values for 'EmailAddressID' on 'DockyardAccountDO' are not allowed. Duplicated value: '1'")]
        public void TestDuplicateUserEmailIDRejected()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EmailAddressRepository.Add(new EmailAddressDO() {Id = 1});
                uow.UserRepository.Add(new DockyardAccountDO() { EmailAddressID = 1, State = UserState.Active });
                uow.UserRepository.Add(new DockyardAccountDO() { EmailAddressID = 1, State = UserState.Active });
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
                var u = new DockyardAccountDO();
                var user = new DockyardAccount();

                //SETUP                 

                DockyardAccountDO currDockyardAccountDO = new DockyardAccountDO();
                uow.UserRepository.Add(currDockyardAccountDO);
               
                DockyardAccountDO currRetrivedDockyardAccountDO = uow.UserRepository.GetQuery().FirstOrDefault(uu => currDockyardAccountDO.EmailAddressID == uu.EmailAddressID);
            }
        }
    }
}
