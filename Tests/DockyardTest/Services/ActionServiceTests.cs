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
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActionRegistrationDO> _pr1Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO(){ ActionType = "Write", Version = "1.0"}, new ActionRegistrationDO(){ ActionType = "Read", Version = "1.0"} };
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
            _fixtureData = new FixtureData(_uow);
        }

        [Test]
        public void CanRetrieveActionsForAccount()
        {
            var dockyardAccount = _fixtureData.TestUser1();
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
        public void Register_NewAction_RegisterNewAction()
        {
            var newAction = _fixtureData.TestActionRegistration1();

            _action.Register(newAction.ActionType, newAction.ParentPluginRegistration, newAction.Version);
            var savedAction = _uow.ActionRegistrationRepository.GetQuery().FirstOrDefault(i => i.Id == newAction.Id);

            Assert.AreEqual(savedAction.ActionType, savedAction.ActionType);
            Assert.AreEqual(savedAction.ParentPluginRegistration, savedAction.ParentPluginRegistration);
            Assert.AreEqual(savedAction.Version, savedAction.Version);
        }

        [Test]
        public void Register_ExistingAction_DoNothing()
        {
            var newAction = _fixtureData.TestActionRegistration1();

            _action.Register(newAction.ActionType, newAction.ParentPluginRegistration, newAction.Version);
            int totalRecords = _uow.ActionRegistrationRepository.GetQuery().Count();
            _action.Register(newAction.ActionType, newAction.ParentPluginRegistration, newAction.Version);
            int totalRecordsAfterExistingRegister = _uow.ActionRegistrationRepository.GetQuery().Count();

            Assert.AreEqual(totalRecords, totalRecordsAfterExistingRegister);
        }
    }
}
