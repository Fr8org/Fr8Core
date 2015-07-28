using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Core.Managers;
using Core.StructureMap;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using StructureMap;

namespace DockyardTest.Models
{
    [TestFixture]
    public class CustomerTests : BaseTest
    {
        [Test]
        [Category("Customer")]
        public void Customer_Add_CanCreateUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP
                //create a customer from fixture data
                DockyardAccountDO curDockyardAccountDO = fixture.TestUser1();

                //EXECUTE
                uow.UserRepository.Add(curDockyardAccountDO);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                DockyardAccountDO savedDockyardAccountDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == curDockyardAccountDO.Id);
                Assert.AreEqual(curDockyardAccountDO.FirstName, savedDockyardAccountDO.FirstName);
                Assert.AreEqual(curDockyardAccountDO.EmailAddress, savedDockyardAccountDO.EmailAddress);

            }

        }
    }
}
