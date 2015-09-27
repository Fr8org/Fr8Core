using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Action = Core.Services.Action;
using System.Threading.Tasks;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class ActionServiceTests : BaseTest
    {
        private TestActionService _action;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActivityTemplateDO> _pr1Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActivityTemplateDO> _pr2Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "SQL Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "SQL Read", Version = "1.0" } };

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _action = new TestActionService();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData(_uow);
        }

        [Test]
        public void Action_Configure_ExistingActionShouldBeUpdatedWithNewAction()
        {
            //Arrange
            ActionDO curActionDO = FixtureData.IntegrationTestAction();
            UpdateDatabase(curActionDO);

            Mock<IRestfulServiceClient> restClientMock = Mock.Get(_action.RestfulServiceClient);
            ActionDTO actionDto = Mapper.Map<ActionDTO>(curActionDO);

            //set the new name
            actionDto.Name = "NewActionFromServer";
            restClientMock.Setup(rc => rc.PostAsync(It.IsAny<Uri>(), It.IsAny<object>()))
                .Returns(() => Task.FromResult<string>(JsonConvert.SerializeObject(actionDto)));

            //Act
            var returnedAction = _action.Configure(curActionDO);

            //Assert
            //get the action from the database
            var updatedActionDO = _uow.ActionRepository.GetByKey(returnedAction.Id);
            Assert.IsNotNull(updatedActionDO);
            Assert.AreEqual(updatedActionDO.Name, actionDto.Name);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Action_Configure_WithNullActionTemplate_ThrowsArgumentNullException()
        {
            var _service = new Action();
            Assert.IsNotNull(_service.Configure(null));
        }

        [Test]
        public void CanCRUDActions()
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IAction action = new Action();
                var origActionDO = new FixtureData(uow).TestAction3();

                //Add
                action.SaveOrUpdateAction(origActionDO);

                //Get
                var actionDO = action.GetById(origActionDO.Id);
                Assert.AreEqual(origActionDO.Name, actionDO.Name);
                Assert.AreEqual(origActionDO.Id, actionDO.Id);
                Assert.AreEqual(origActionDO.CrateStorage, actionDO.CrateStorage);

                Assert.AreEqual(origActionDO.Ordering, actionDO.Ordering);

                //Delete
                action.Delete(actionDO.Id);
            }
        }

        [Test]
        public void CanParsePayload()
        {
            var envelope = new DocuSignEnvelope();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var payloadMappings = FixtureData.ListFieldMappings;

            List<EnvelopeDataDTO> envelopeData = FixtureData.TestEnvelopeDataList2(envelopeId);

            var result = envelope.ExtractPayload(payloadMappings, envelopeId, envelopeData);

            Assert.AreEqual("Johnson", result.Where(p => p.Key == "Doctor").Single().Value);
            Assert.AreEqual("Marthambles", result.Where(p => p.Key == "Condition").Single().Value);
        }

        [Test]
        public void CanLogIncidentWhenFieldIsMissing()
        {
            IncidentReporter incidentReporter = new IncidentReporter();
            incidentReporter.SubscribeToAlerts();

            var envelope = new DocuSignEnvelope();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var payloadMappings = FixtureData.ListFieldMappings2; //Wrong mappings

            List<EnvelopeDataDTO> envelopeData = FixtureData.TestEnvelopeDataList2(envelopeId);
            var result = envelope.ExtractPayload(payloadMappings, envelopeId, envelopeData);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsTrue(uow.IncidentRepository.GetAll().Any(i => i.PrimaryCategory == "Envelope"));
            }
        }

        [Test]
        public void CanProcessDocuSignTemplate()
        {
            // Test.
            Action action = new Action();
            var processTemplate = FixtureData.TestProcessTemplate2();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();
            actionDo.ActivityTemplate.Plugin.Endpoint = "http://localhost:53234/actions/configure";
            ProcessDO procesDO = FixtureData.TestProcess1();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.ActionRepository.Add(actionDo);
                uow.ActionListRepository.Add((ActionListDO)actionDo.ParentActivity);
                uow.ProcessRepository.Add(((ActionListDO)actionDo.ParentActivity).Process);
                uow.SaveChanges();
            }

            action.PrepareToExecute(actionDo, procesDO).Wait();

            //Ensure that no Incidents were registered
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsFalse(uow.IncidentRepository.GetAll().Any(i => i.PrimaryCategory == "Envelope"));
            }

            //We use a mock of IPluginTransmitter. Get that mock and check that 
            //PostActionAsync was called with the correct attributes
            var transmitter = ObjectFactory.GetInstance<IPluginTransmitter>(); //it is configured as a singleton so we get the "used" instance
            var mock = Mock.Get<IPluginTransmitter>(transmitter);

            //TODO: Fix this line according to v2 changes
            mock.Verify(e => e.PostActionAsync(It.Is<string>(s => s == "testaction"),
                It.Is<ActionDTO>(a => true), It.IsAny<PayloadDTO>()));
        }

        [Test]
        public void Process_ActionNotUnstarted_ThrowException()
        {
            ActionDO actionDo = FixtureData.TestAction9();
            Action _action = ObjectFactory.GetInstance<Action>();
            ProcessDO procesDo = FixtureData.TestProcess1();
            Assert.AreEqual("Action ID: 2 status is 4.", _action.PrepareToExecute(actionDo, procesDo).Exception.InnerException.Message);
        }

        [Test, Ignore("Ignored execution related tests. Refactoring is going on")]
        public void Process_ReturnJSONDispatchError_ActionStateError()
        {
            ActionDO actionDO = FixtureData.IntegrationTestAction();
            ProcessDO procesDo = FixtureData.TestProcess1();
            var pluginClientMock = new Mock<IPluginTransmitter>();
            pluginClientMock.Setup(s => s.PostActionAsync(It.IsAny<string>(), It.IsAny<ActionDTO>(), It.IsAny<PayloadDTO>())).ReturnsAsync(@"{ ""error"" : { ""ErrorCode"": ""0000"" }}");
            ObjectFactory.Configure(cfg => cfg.For<IPluginTransmitter>().Use(pluginClientMock.Object));
            //_action = ObjectFactory.GetInstance<IAction>();

            _action.PrepareToExecute(actionDO, procesDo);

            Assert.AreEqual(ActionState.Error, actionDO.ActionState);
        }

        [Test]
        public void Process_ReturnJSONDispatchNotError_ActionStateCompleted()
        {
            ActionDO actionDO = FixtureData.IntegrationTestAction();
            actionDO.ActivityTemplate.Plugin.Endpoint = "http://localhost:53234/actions/configure";
            ProcessDO procesDO = FixtureData.TestProcess1();
            var pluginClientMock = new Mock<IPluginTransmitter>();
            pluginClientMock.Setup(s => s.PostActionAsync(It.IsAny<string>(), It.IsAny<ActionDTO>(), It.IsAny<PayloadDTO>())).ReturnsAsync(@"{ ""success"" : { ""ID"": ""0000"" }}");
            ObjectFactory.Configure(cfg => cfg.For<IPluginTransmitter>().Use(pluginClientMock.Object));
            //_action = ObjectFactory.GetInstance<IAction>();

            _action.PrepareToExecute(actionDO, procesDO);

            Assert.AreEqual(ActionState.Active, actionDO.ActionState);
        }

        [Test]
        public void Process_ActionUnstarted_ShouldBeCompleted()
        {
            //Arrange
            ActionDO actionDo = FixtureData.TestActionUnstarted();
            actionDo.ActivityTemplate.Plugin.Endpoint = "http://localhost:53234/actions/configure";
            actionDo.CrateStorage = JsonConvert.SerializeObject(new ActionDTO());
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(_action.RestfulServiceClient);

            ActionDTO actionDto = Mapper.Map<ActionDTO>(actionDo);
            restClientMock.Setup(rc => rc.PostAsync(It.IsAny<Uri>(), It.IsAny<object>()))
                .Returns(() => Task.FromResult<string>(JsonConvert.SerializeObject(actionDto)));

            ProcessDO procesDO = FixtureData.TestProcess1();

            //Act
            var response = _action.PrepareToExecute(actionDo, procesDO);

            //Assert
            Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test]
        public void Authenticate_AuthorizationTokenIsActive_ReturnsAuthorizationToken()
        {
            var curActionDO = FixtureData.TestActionAuthenticate1();
            var curActionListDO = (ActionListDO)curActionDO.ParentActivity;


            AuthorizationTokenDO curAuthorizationTokenDO = FixtureData.TestActionAuthenticate2();
            curAuthorizationTokenDO.Plugin = curActionDO.ActivityTemplate.Plugin;
            curAuthorizationTokenDO.UserDO = curActionListDO.Process.ProcessTemplate.DockyardAccount;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.AuthorizationTokenRepository.Add(curAuthorizationTokenDO);
                uow.SaveChanges();
            }
            string result = _action.Authenticate(curActionDO);
            Assert.AreEqual("TestToken", result);
        }

        [Test]
        public void Authenticate_AuthorizationTokenIsRevoke_RedirectsToPluginAuthenticate()
        {
            var curActionDO = FixtureData.TestActionAuthenticate1();
            var curActionListDO = (ActionListDO)curActionDO.ParentActivity;

            AuthorizationTokenDO curAuthorizationTokenDO = FixtureData.TestActionAuthenticate3();
            curAuthorizationTokenDO.Plugin = curActionDO.ActivityTemplate.Plugin;
            curAuthorizationTokenDO.UserDO = curActionListDO.Process.ProcessTemplate.DockyardAccount;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.AuthorizationTokenRepository.Add(curAuthorizationTokenDO);
                uow.SaveChanges();
            }
            string result = _action.Authenticate(curActionDO);
            Assert.AreEqual("AuthorizationToken", result);
        }

        [Test]
        public void AddCrate_AddCratesDTO_UpdatesActionCratesStorage()
        {
            ActionDO actionDO = FixtureData.TestAction23();

            _action.AddCrate(actionDO, FixtureData.CrateStorageDTO().CrateDTO);

            Assert.IsNotEmpty(actionDO.CrateStorage);
        }

        private void UpdateDatabase(ActionDO curActionDo)
        {

            curActionDo.ActivityTemplate.Plugin.Endpoint = "pluginDocusign";
            _uow.ActivityTemplateRepository.Add(curActionDo.ActivityTemplate);
            _uow.SaveChanges();

            _uow.ProcessTemplateRepository.Add(FixtureData.TestProcessTemplate1());

            ActionListDO parentActivity = (ActionListDO) curActionDo.ParentActivity;
            parentActivity.Process.ProcessTemplateId = 1;
            _uow.ProcessRepository.Add(parentActivity.Process);
            _uow.SaveChanges();

            _uow.ActionListRepository.Add(parentActivity);
            _uow.SaveChanges();

            _uow.ActionRepository.Add(curActionDo);
            _uow.SaveChanges();
        }
    }

    internal class TestActionService : Action
    {
        private IRestfulServiceClient _restfulServiceClient;

        internal IRestfulServiceClient RestfulServiceClient
        {
            get
            {
                if (_restfulServiceClient == null)
                {
                    _restfulServiceClient = new Mock<IRestfulServiceClient>(MockBehavior.Default).Object;
                }

                return _restfulServiceClient;
            }
            private set { _restfulServiceClient = value; }
        }

        protected override IRestfulServiceClient PrepareRestfulClient()
        {
            if (_restfulServiceClient == null)
            {
                _restfulServiceClient = new Mock<IRestfulServiceClient>(MockBehavior.Default).Object;
            }

            return _restfulServiceClient;
        }
    }
}
