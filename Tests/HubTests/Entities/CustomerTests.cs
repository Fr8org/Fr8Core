using System.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Managers;
using Hub.StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Models
{
    [TestFixture]
    public class CustomerTests : BaseTest
    {
        [Test]
        [Category("User")]
        public void Customer_Add_CanCreateUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                //SETUP
                //create a customer from fixture data
                Fr8AccountDO curDockyardAccountDO = FixtureData.TestUser1();

                //EXECUTE
                uow.UserRepository.Add(curDockyardAccountDO);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                Fr8AccountDO savedDockyardAccountDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == curDockyardAccountDO.Id);
                Assert.AreEqual(curDockyardAccountDO.FirstName, savedDockyardAccountDO.FirstName);
                Assert.AreEqual(curDockyardAccountDO.EmailAddress, savedDockyardAccountDO.EmailAddress);

            }

        }
    }
}
