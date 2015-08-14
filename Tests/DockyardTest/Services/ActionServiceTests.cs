using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.PluginRegistrations;
using Data.Interfaces;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Entities;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class ActionServiceTests : BaseTest
    {
        private IAction _action;
        private IUnitOfWork _uow;
        private readonly IEnumerable<ActionRegistrationDO> _pr1Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO() { ActionType = "Write", Version = "1.0" }, new ActionRegistrationDO() { ActionType = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActionRegistrationDO> _pr2Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO() { ActionType = "SQL Write", Version = "1.0" }, new ActionRegistrationDO() { ActionType = "SQL Read", Version = "1.0" } };


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var pluginRegistration1Mock = new Mock<IPluginRegistration>();
            pluginRegistration1Mock
                .SetupGet(pr => pr.AvailableCommands)
                .Returns(_pr1Actions);
            var pluginRegistration2Mock = new Mock<IPluginRegistration>();
            pluginRegistration2Mock
                .SetupGet(pr => pr.AvailableCommands)
                .Returns(_pr2Actions);
            var subscriptionMock = new Mock<ISubscription>();
            subscriptionMock
                .Setup(s => s.GetAuthorizedPlugins(It.IsAny<IDockyardAccountDO>()))
                .Returns(new[]
                {
                    pluginRegistration1Mock.Object,
                    pluginRegistration2Mock.Object
                });
            ObjectFactory.Configure(cfg => cfg.For<ISubscription>().Use(subscriptionMock.Object));
            _action = ObjectFactory.GetInstance<IAction>();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            
        }

        [Test]
        public void CanRetrieveActionsForAccount()
        {
            var dockyardAccount = FixtureData.TestUser1();
            var result = _action.GetAvailableActions(dockyardAccount).ToArray();
            var expectedResult = _pr1Actions.Concat(_pr2Actions).OrderBy(s => s.ActionType, StringComparer.OrdinalIgnoreCase).ToArray();
            Assert.AreEqual(expectedResult.Length, result.Length, "Actions list length is different.");
            Assert.That(Enumerable
                .Zip(
                    result, expectedResult,
                    (s1, s2) => string.Equals(s1.ActionType, s2.ActionType, StringComparison.Ordinal))
                .All(b => b),
                "Actions lists are different.");
        }
        [Test]
        public void ActionService_GetConfigurationSettings_CanGetCorrectJson()
        {
            var curActionRegistration = FixtureData.TestActionRegistrationDO1();
            string curJsonResult = "{\"configurationSettings\":[{\"textField\": {\"name\": \"connection_string\",\"required\":true,\"value\":\"\",\"fieldLabel\":\"SQL Connection String\",}}]}";
            Assert.AreEqual(_action.GetConfigurationSettings(curActionRegistration).ConfigurationSettings, curJsonResult);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionService_NULL_ActionRegistration()
        {
            var _service = new Core.Services.Action();
            Assert.IsNotNull(_service.GetConfigurationSettings(null));
        }
    }
}
