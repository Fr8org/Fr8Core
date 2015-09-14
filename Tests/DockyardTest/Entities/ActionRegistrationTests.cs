using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class ActionTemplateTests : BaseTest
    {
        [Test]
        [Category("ActivityTemplate")]
        public void ActivityTemplate_Add_CanCreateActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                
                var actionTemplateDO = FixtureData.TestActivityTemplate1();

                uow.ActivityTemplateRepository.Add(actionTemplateDO);
                uow.SaveChanges();

                var savedActivityTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == actionTemplateDO.Id);
                Assert.NotNull(savedActivityTemplateDO);

                Assert.AreEqual(actionTemplateDO.Name, savedActivityTemplateDO.Name);
                Assert.AreEqual(actionTemplateDO.Plugin.Endpoint, savedActivityTemplateDO.Plugin.Endpoint);

            }
        }

        [Test]
        [Category("ActivityTemplate")]
        public void ActivityTemplate_Remove_CanRemoveActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                var actionTemplateDO = FixtureData.TestActivityTemplate1();

                uow.ActivityTemplateRepository.Add(actionTemplateDO);
                uow.SaveChanges();

                var savedActivityTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == actionTemplateDO.Id);


                Assert.NotNull(savedActivityTemplateDO);
                Assert.AreEqual(actionTemplateDO.Name, savedActivityTemplateDO.Name);
                Assert.AreEqual(actionTemplateDO.Version, savedActivityTemplateDO.Version);

                // remove saved instance
                uow.ActivityTemplateRepository.Remove(savedActivityTemplateDO);
                uow.SaveChanges();

                var deletedActivityTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == savedActivityTemplateDO.Id);

                Assert.IsNull(deletedActivityTemplateDO);

            }
        }
    }
    
}
