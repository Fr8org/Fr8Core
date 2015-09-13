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
        [Category("ActionTemplate")]
        public void ActionTemplate_Add_CanCreateActionTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                
                var actionTemplateDO = FixtureData.TestActionTemplate1();

                uow.ActionTemplateRepository.Add(actionTemplateDO);
                uow.SaveChanges();

                var savedActionTemplateDO = uow.ActionTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == actionTemplateDO.Id);
                Assert.NotNull(savedActionTemplateDO);

                Assert.AreEqual(actionTemplateDO.Name, savedActionTemplateDO.Name);
                Assert.AreEqual(actionTemplateDO.Plugin.BaseEndPoint, savedActionTemplateDO.Plugin.BaseEndPoint);

            }
        }

        [Test]
        [Category("ActionTemplate")]
        public void ActionTemplate_Remove_CanRemoveActionTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                var actionTemplateDO = FixtureData.TestActionTemplate1();

                uow.ActionTemplateRepository.Add(actionTemplateDO);
                uow.SaveChanges();

                var savedActionTemplateDO = uow.ActionTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == actionTemplateDO.Id);


                Assert.NotNull(savedActionTemplateDO);
                Assert.AreEqual(actionTemplateDO.Name, savedActionTemplateDO.Name);
                Assert.AreEqual(actionTemplateDO.Version, savedActionTemplateDO.Version);

                // remove saved instance
                uow.ActionTemplateRepository.Remove(savedActionTemplateDO);
                uow.SaveChanges();

                var deletedActionTemplateDO = uow.ActionTemplateRepository.GetQuery().FirstOrDefault(u => u.Id == savedActionTemplateDO.Id);

                Assert.IsNull(deletedActionTemplateDO);

            }
        }
    }
    
}
