using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Entities
{
    [TestFixture]
    public class ActivityTemplateTests : BaseTest
    {
        [Test]
        [Category("ActivityTemplate")]
        public void ActivityTemplate_Add_CanCreateActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                
                var activityTemplateDO = FixtureData.TestActivityTemplate1();

                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();

                var savedActivityTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == activityTemplateDO.Id);
                Assert.NotNull(savedActivityTemplateDO);

                Assert.AreEqual(activityTemplateDO.Name, savedActivityTemplateDO.Name);
                Assert.AreEqual(activityTemplateDO.Terminal.Endpoint, savedActivityTemplateDO.Terminal.Endpoint);

            }
        }

        [Test]
        [Category("ActivityTemplate")]
        public void ActivityTemplate_Remove_CanRemoveActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                var activityTemplateDO = FixtureData.TestActivityTemplate1();

                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                uow.SaveChanges();

                var savedActivityTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == activityTemplateDO.Id);


                Assert.NotNull(savedActivityTemplateDO);
                Assert.AreEqual(activityTemplateDO.Name, savedActivityTemplateDO.Name);
                Assert.AreEqual(activityTemplateDO.Version, savedActivityTemplateDO.Version);

                // remove saved instance
                uow.ActivityTemplateRepository.Remove(savedActivityTemplateDO);
                uow.SaveChanges();

                var deletedActivityTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == savedActivityTemplateDO.Id);

                Assert.IsNull(deletedActivityTemplateDO);

            }
        }
    }
    
}
