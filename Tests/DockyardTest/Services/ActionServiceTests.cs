using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
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

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class ActionServiceTests : BaseTest
    {
        private IAction _action;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActivityTemplateDO> _pr1Actions = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActivityTemplateDO> _pr2Actions = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "SQL Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "SQL Read", Version = "1.0" } };

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            //var pluginRegistration1Mock = new Mock<IPluginRegistration>();
            //pluginRegistration1Mock
            //    .SetupGet(pr => pr.AvailableActions)
            //    .Returns(_pr1Actions);
            //var pluginRegistration2Mock = new Mock<IPluginRegistration>();
            //pluginRegistration2Mock
            //    .SetupGet(pr => pr.AvailableActions)
            //    .Returns(_pr2Actions);
            //var subscriptionMock = new Mock<ISubscription>();
            //subscriptionMock
            //    .Setup(s => s.GetAuthorizedPlugins(It.IsAny<IDockyardAccountDO>()))
            //    .Returns(new[]
            //    {
            //        pluginRegistration1Mock.Object,
            //        pluginRegistration2Mock.Object
            //    });
            //ObjectFactory.Configure(cfg => cfg.For<ISubscription>().Use(subscriptionMock.Object));
            _action = ObjectFactory.GetInstance<IAction>();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData(_uow);
        }

        [Test, Ignore("Vas Ignored as part of V2 Changes")]
        public void ActionService_GetConfigurationSettings_CanGetCorrectJson()
        {
            var expectedResult = FixtureData.TestConfigurationSettings();
            var curActionDO = FixtureData.TestAction22();
            string curJsonResult = _action.GetConfigurationSettings(curActionDO);
            CrateStorageDTO result = Newtonsoft.Json.JsonConvert.DeserializeObject<CrateStorageDTO>(curJsonResult);
            Assert.AreEqual(1, result.Fields.Count);
            Assert.AreEqual(expectedResult.Fields[0].FieldLabel, result.Fields[0].FieldLabel);
            Assert.AreEqual(expectedResult.Fields[0].Type, result.Fields[0].Type);
            Assert.AreEqual(expectedResult.Fields[0].Name, result.Fields[0].Name);
            Assert.AreEqual(expectedResult.Fields[0].Required, result.Fields[0].Required);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionService_NULL_ActionTemplate()
        {
            var _service = new Action();
            Assert.IsNotNull(_service.GetConfigurationSettings(null));
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
                Assert.AreEqual(origActionDO.ConfigurationStore, actionDO.ConfigurationStore);
                Assert.AreEqual(origActionDO.FieldMappingSettings, actionDO.FieldMappingSettings);
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

        [Test, Ignore("Vas, Ignored as part of V2 changes")]
        public void CanProcessDocuSignTemplate()
        {
            // Test.
            Action action = new Action();
            var processTemplate = FixtureData.TestProcessTemplate2();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.ActionRepository.Add(actionDo);
                uow.ActionListRepository.Add((ActionListDO)actionDo.ParentActivity);
                uow.ProcessRepository.Add(((ActionListDO)actionDo.ParentActivity).Process);
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

        [Test, Ignore("Vas, Ignored as part of V2 changes")]
        public void CanSavePayloadMappingToActionTabe()
        {
            Action action = new Action();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();
            var processTemplate = FixtureData.TestProcessTemplate2();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.ActionRepository.Add(actionDo);
                uow.ActionListRepository.Add((ActionListDO)actionDo.ParentActivity);
                uow.ProcessRepository.Add(((ActionListDO)actionDo.ParentActivity).Process);
                uow.SaveChanges();
            }

            action.Process(actionDo).Wait();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionDo = uow.ActionRepository.FindOne((a) => true);
                Assert.NotNull(curActionDo);
                var curActionDto = Mapper.Map<ActionPayloadDTO>(curActionDo);
                Assert.IsTrue(IsPayloadValid(curActionDto));                
            }
        }

        private bool IsPayloadValid(ActionPayloadDTO dto)
        {
            return (dto.PayloadMappings.Any(m => m.Name == "Doctor" && m.Value == "Johnson") &&
                dto.PayloadMappings.Any(m => m.Name == "Condition" && m.Value == "Marthambles"));
        }

        [Test,Ignore]
        public void Process_ActionNotUnstarted_ThrowException()
        {
            ActionDO actionDo = FixtureData.TestAction9();
            Action _action = ObjectFactory.GetInstance<Action>();

            Assert.AreEqual("Action ID: 2 status is 4.", _action.Process(actionDo).Exception.InnerException.Message);
        }

        [Test,Ignore]
        public void Process_ReturnJSONDispatchError_ActionStateError()
        {
            ActionDO actionDO = FixtureData.IntegrationTestAction();
            var pluginClientMock = new Mock<IPluginTransmitter>();
            pluginClientMock.Setup(s => s.PostActionAsync(It.IsAny<string>(), (ActionPayloadDTO)It.IsAny<object>())).ReturnsAsync(@"{ ""error"" : { ""ErrorCode"": ""0000"" }}");
            ObjectFactory.Configure(cfg => cfg.For<IPluginTransmitter>().Use(pluginClientMock.Object));
            _action = ObjectFactory.GetInstance<IAction>();

            _action.Process(actionDO);

            Assert.AreEqual(ActionState.Error, actionDO.ActionState);
        }

        [Test, Ignore("Vas, Ignored as part of V2 changes")]
        public void Process_ReturnJSONDispatchNotError_ActionStateCompleted()
        {
            ActionDO actionDO = FixtureData.IntegrationTestAction();
            var pluginClientMock = new Mock<IPluginTransmitter>();
            pluginClientMock.Setup(s => s.PostActionAsync(It.IsAny<string>(), (ActionPayloadDTO)It.IsAny<object>())).ReturnsAsync(@"{ ""success"" : { ""ID"": ""0000"" }}");
            ObjectFactory.Configure(cfg => cfg.For<IPluginTransmitter>().Use(pluginClientMock.Object));
            _action = ObjectFactory.GetInstance<IAction>();

            _action.Process(actionDO);

            Assert.AreEqual(ActionState.Completed, actionDO.ActionState);
        }

        [Test, Ignore("Vas, Ignored as part of V2 changes")]
        public void Process_ActionUnstarted_ShouldBeCompleted()
        {
            ActionDO actionDo = FixtureData.TestActionUnstarted();
            Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();
            var response = _action.Process(actionDo);
            Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test, Ignore("Vas, ignored due to V2 changes")]
        public void Dispatch_PayloadDTO_ShouldBeDispatched()
        {
            //ActionDO actionDo = FixtureData.TestActionUnstarted();
            //Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();
            //var pluginRegistration = BasePluginRegistration.GetPluginType(actionDo);
            //Uri baseUri = new Uri(pluginRegistration.BaseUrl, UriKind.Absolute);
            //var response = _action.Dispatch(actionDo, baseUri);
            //Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));

        }

        //this test is being phased out
        [Test,Ignore]
        public void GetAvailableActions_ReturnsActionsForAccount()
        {
            //const string unavailablePluginName = "UnavailablePlugin";
            //const string noAccessPluginName = "NoAccessPlugin";
            //const string userAccessPluginName = "AvailableWithUserAccessPlugin";
            //const string adminAccessPluginName = "AvailableWithAdminAccessPlugin";
            //var unavailablePluginRegistration = new Mock<IPluginRegistration>();
            //var noAccessPluginRegistration = new Mock<IPluginRegistration>();
            //var userAccessPluginRegistration = new Mock<IPluginRegistration>();
            //var adminAccessPluginRegistration = new Mock<IPluginRegistration>();
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(unavailablePluginRegistration.Object).Named(unavailablePluginName));
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(noAccessPluginRegistration.Object).Named(noAccessPluginName));
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(userAccessPluginRegistration.Object).Named(userAccessPluginName));
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(adminAccessPluginRegistration.Object).Named(adminAccessPluginName));
            //var account = new DockyardAccountDO()
            //{
            //    Subscriptions = new List<SubscriptionDO>()
            //    {
            //        new SubscriptionDO()
            //        {
            //            AccessLevel = AccessLevel.None,
            //            Plugin = new PluginDO() {Name = noAccessPluginName}
            //        },
            //        new SubscriptionDO()
            //        {
            //            AccessLevel = AccessLevel.User,
            //            Plugin = new PluginDO() {Name = userAccessPluginName}
            //        },
            //        new SubscriptionDO()
            //        {
            //            AccessLevel = AccessLevel.Admin,
            //            Plugin = new PluginDO() {Name = adminAccessPluginName}
            //        },
            //    }
            //};

            //Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();
            //List<ActionTemplateDO> curActionTemplateDO = _action.GetAvailableActions(account).ToList();

            ////Assert
            //Assert.AreEqual(4, curActionTemplateDO.Count);
            //Assert.That(curActionTemplateDO, Is.Ordered.By("ActionType"));


        }

        [Test]
        public void Authenticate_AuthorizationTokenIsActive_ReturnsAuthorizationToken()
        {
            var curActionDO = FixtureData.TestActionAuthenticate1();
            var curActionListDO = (ActionListDO)curActionDO.ParentActivity;


            AuthorizationTokenDO curAuthorizationTokenDO = FixtureData.TestActionAuthenticate2();
            curAuthorizationTokenDO.Plugin = curActionDO.ActionTemplate.Plugin;
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
            curAuthorizationTokenDO.Plugin = curActionDO.ActionTemplate.Plugin;
            curAuthorizationTokenDO.UserDO = curActionListDO.Process.ProcessTemplate.DockyardAccount;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.AuthorizationTokenRepository.Add(curAuthorizationTokenDO);
                uow.SaveChanges();
            }
            string result = _action.Authenticate(curActionDO);
            Assert.AreEqual("AuthorizationToken", result);
        }

     




    }
}
