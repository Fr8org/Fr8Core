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
            Core.Services.Action action = new Core.Services.Action();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09",
                payloadMappings = FixtureData.PayloadMappings1;

            List<EnvelopeDataDTO> envelopeData = FixtureData.TestEnvelopeDataList2(envelopeId);

            string result = action.ParsePayloadMappings(payloadMappings, envelopeId, envelopeData);

            var obj = JObject.Parse(result)["payload"];
            Assert.AreEqual("Johnson", obj["Doctor"].Value<string>());
            Assert.AreEqual("Marthambles", obj["Condition"].Value<string>());
        }

        [Test]
        public void CanLogIncidentWhenFieldIsMissing()
        {
            Core.Services.Action action = new Core.Services.Action();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09",
                payloadMappings = FixtureData.PayloadMappings2; //Wrong mappings

            List<EnvelopeDataDTO> envelopeData = FixtureData.TestEnvelopeDataList2(envelopeId);
            string result = action.ParsePayloadMappings(payloadMappings, envelopeId, envelopeData);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsTrue(uow.IncidentRepository.GetAll().Any(i => i.PrimaryCategory == "Envelope"));
            }
        } 
    }
}
