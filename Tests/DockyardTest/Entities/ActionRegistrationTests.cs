using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class ActionRegistrationTests : BaseTest
    {
        [Test]
        [Category("ActionRegistration")]
        public void ActionRegistration_Add_CanCreateActionRegistration()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                
                var actionRegistrationDO = fixture.TestActionRegistration1();

                uow.ActionRegistrationRepository.Add(actionRegistrationDO);
                uow.SaveChanges();

                var savedActionRegistrationDO = uow.ActionRegistrationRepository.GetQuery().FirstOrDefault(u => u.Id == actionRegistrationDO.Id);
                Assert.NotNull(savedActionRegistrationDO);

                Assert.AreEqual(actionRegistrationDO.ActionType, savedActionRegistrationDO.ActionType);
                Assert.AreEqual(actionRegistrationDO.ParentPluginRegistration, savedActionRegistrationDO.ParentPluginRegistration);

            }
        }

        [Test]
        [Category("ActionRegistration")]
        public void ActionRegistration_Remove_CanRemoveActionRegistration()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                var actionRegistrationDO = fixture.TestActionRegistration1();

                uow.ActionRegistrationRepository.Add(actionRegistrationDO);
                uow.SaveChanges();

                var savedActionRegistrationDO = uow.ActionRegistrationRepository.GetQuery().FirstOrDefault(u => u.Id == actionRegistrationDO.Id);


                Assert.NotNull(savedActionRegistrationDO);
                Assert.AreEqual(actionRegistrationDO.ActionType, savedActionRegistrationDO.ActionType);
                Assert.AreEqual(actionRegistrationDO.Version, savedActionRegistrationDO.Version);

                // remove saved instance
                uow.ActionRegistrationRepository.Remove(savedActionRegistrationDO);
                uow.SaveChanges();

                var deletedActionRegistrationDO = uow.ActionRegistrationRepository.GetQuery().FirstOrDefault(u => u.Id == savedActionRegistrationDO.Id);

                Assert.IsNull(deletedActionRegistrationDO);

            }
        }
    }
    
}
