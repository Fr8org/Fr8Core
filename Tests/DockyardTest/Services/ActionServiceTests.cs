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
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;
using Data.States;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers;
using Data.Wrappers;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class ActionServiceTests : BaseTest
    {
        private IAction _action;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActionRegistrationDO> _pr1Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO() { ActionType = "Write", Version = "1.0" }, new ActionRegistrationDO() { ActionType = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActionRegistrationDO> _pr2Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO() { ActionType = "SQL Write", Version = "1.0" }, new ActionRegistrationDO() { ActionType = "SQL Read", Version = "1.0" } };


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var pluginRegistration1Mock = new Mock<IPluginRegistration>();
            pluginRegistration1Mock
                .SetupGet(pr => pr.AvailableActions)
                .Returns(_pr1Actions);
            var pluginRegistration2Mock = new Mock<IPluginRegistration>();
            pluginRegistration2Mock
                .SetupGet(pr => pr.AvailableActions)
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

        [Test]
        public void CanCRUDActions()
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IAction action = new Core.Services.Action();
                var origActionDO = new FixtureData(uow).TestAction3();

                //Add
                action.SaveOrUpdateAction(origActionDO);

                //Get
                var actionDO = action.GetById(origActionDO.Id);
                Assert.AreEqual(origActionDO.ActionType, actionDO.ActionType);
                Assert.AreEqual(origActionDO.Id, actionDO.Id);
                Assert.AreEqual(origActionDO.ConfigurationSettings, actionDO.ConfigurationSettings);
                Assert.AreEqual(origActionDO.FieldMappingSettings, actionDO.FieldMappingSettings);
                Assert.AreEqual(origActionDO.UserLabel, actionDO.UserLabel);
                Assert.AreEqual(origActionDO.Ordering, actionDO.Ordering);

                //Delete
                action.Delete(actionDO.Id);
            }
        }

        [Test]
        public void CanParsePayload()
        {
            var envelope = new DocuSignEnvelope();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09",
                payloadMappings = FixtureData.FieldMappings;

            List<EnvelopeDataDTO> envelopeData = FixtureData.TestEnvelopeDataList2(envelopeId);

            var result = envelope.ExtractPayload(payloadMappings, envelopeId, envelopeData);

            Assert.AreEqual("Johnson", result.Where(p => p.Name == "Doctor").Single().Value);
            Assert.AreEqual("Marthambles", result.Where(p => p.Name == "Condition").Single().Value);
        }

        [Test]
        public void CanLogIncidentWhenFieldIsMissing()
        {
            IncidentReporter incidentReporter = new IncidentReporter();
            incidentReporter.SubscribeToAlerts();

            var envelope = new DocuSignEnvelope();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09",
                payloadMappings = FixtureData.FieldMappings2; //Wrong mappings

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
            Core.Services.Action action = new Core.Services.Action();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionRepository.Add(actionDo);
                uow.ActionListRepository.Add(actionDo.ActionList);
                uow.ProcessRepository.Add(actionDo.ActionList.Process);
                uow.SaveChanges();
            }

            action.Process(actionDo).Wait();

            //Ensure that no Incidents were registered
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsFalse(uow.IncidentRepository.GetAll().Any(i => i.PrimaryCategory == "Envelope"));
            }

            //We use a mock of IPluginTransmitter. Get that mock and check that 
            //PostActionAsync was called with the correct attributes
            var transmitter = ObjectFactory.GetInstance<IPluginTransmitter>(); //it is configured as a singleton so we get the "used" instance
            var mock = Mock.Get<IPluginTransmitter>(transmitter);
            mock.Verify(e => e.PostActionAsync(It.Is<string>(s => s == "testaction"),
                It.Is<ActionPayloadDTO>(a => IsPayloadValid(a))));
        }

        [Test]
        public void CanSavePayloadMappingToActionTabe()
        {
            Core.Services.Action action = new Core.Services.Action();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionRepository.Add(actionDo);
                uow.ActionListRepository.Add(actionDo.ActionList);
                uow.ProcessRepository.Add(actionDo.ActionList.Process);
                uow.SaveChanges();
            }

            action.Process(actionDo).Wait();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionDo = uow.ActionRepository.FindOne((a) => true);
                Assert.NotNull(curActionDo);
                var curActionDto = AutoMapper.Mapper.Map<ActionPayloadDTO>(curActionDo);
                Assert.IsTrue(IsPayloadValid(curActionDto));                
            }
        }

        private bool IsPayloadValid(ActionPayloadDTO dto)
        {
            return (dto.PayloadMappings.Any(m => m.Name == "Doctor" && m.Value == "Johnson") &&
                dto.PayloadMappings.Any(m => m.Name == "Condition" && m.Value == "Marthambles"));
        }

        [Test]
        public void Process_ActionListNotUnstarted_ThrowException()
        {
            ActionDO actionDo = FixtureData.TestAction7();
            Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();

            Assert.AreEqual("Action ID: 2 status is not unstarted.", _action.Process(actionDo).Exception.InnerException.Message);
        }
    }
}
