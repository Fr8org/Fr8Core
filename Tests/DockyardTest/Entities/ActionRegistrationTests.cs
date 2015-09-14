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

                var savedActionTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == actionTemplateDO.Id);
                Assert.NotNull(savedActionTemplateDO);

                Assert.AreEqual(actionTemplateDO.Name, savedActionTemplateDO.Name);
                Assert.AreEqual(actionTemplateDO.DefaultEndPoint, savedActionTemplateDO.DefaultEndPoint);

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

                var savedActionTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == actionTemplateDO.Id);


                Assert.NotNull(savedActionTemplateDO);
                Assert.AreEqual(actionTemplateDO.Name, savedActionTemplateDO.Name);
                Assert.AreEqual(actionTemplateDO.Version, savedActionTemplateDO.Version);

                // remove saved instance
                uow.ActivityTemplateRepository.Remove(savedActionTemplateDO);
                uow.SaveChanges();

                var deletedActionTemplateDO = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == savedActionTemplateDO.Id);

                Assert.IsNull(deletedActionTemplateDO);

            }
        }
    }
    
}
