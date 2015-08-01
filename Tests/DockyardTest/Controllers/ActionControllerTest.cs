
using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("Controllers.ActionController")]
    public class ActionControllerTest : BaseTest
    {
        [Test]
        public void ActionController_Save_NewActionDo()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange is done as the action repository does not have any Action
                var controller = new ActionController();

                //Act
                controller.Save(new ActionVM()
                {
                    Id = 1,
                    Name = "AzureSqlAction",
                    ActionType = "AzureSql",
                    ConfigurationSettings = "JSON Config Settings",
                    FieldMappingSettings = "JSON Field Mapping Settings",
                    ParentPluginRegistration = "AzureSql"
                });

                //Assert
                Assert.IsNotNull(uow.ActionRepository);
                Assert.IsTrue(uow.ActionRepository.GetAll().Count() > 1);
            }
        }
    }
}
