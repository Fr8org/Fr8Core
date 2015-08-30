using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
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
        private readonly IEnumerable<ActionTemplateDO> _pr1Actions = new List<ActionTemplateDO>() { new ActionTemplateDO() { ActionType = "Write", Version = "1.0" }, new ActionTemplateDO() { ActionType = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActionTemplateDO> _pr2Actions = new List<ActionTemplateDO>() { new ActionTemplateDO() { ActionType = "SQL Write", Version = "1.0" }, new ActionTemplateDO() { ActionType = "SQL Read", Version = "1.0" } };

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
            var expectedResult = FixtureData.TestConfigurationSettings();
            var curActionTemplate = FixtureData.TestActionTemplateDO1();
            string curJsonResult = _action.GetConfigurationSettings(curActionTemplate).ConfigurationSettings;
            ConfigurationSettingsDTO result = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(curJsonResult);
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
                Assert.AreEqual(origActionDO.ConfigurationSettings, actionDO.ConfigurationSettings);
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

        [Test]
        public void CanProcessDocuSignTemplate()
        {
            Action action = new Action();
            var processTemplate = FixtureData.TestProcessTemplate2();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
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
            Action action = new Action();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();
            var processTemplate = FixtureData.TestProcessTemplate2();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
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
                var curActionDto = Mapper.Map<ActionPayloadDTO>(curActionDo);
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
            ActionDO actionDo = FixtureData.TestAction9();
            Action _action = ObjectFactory.GetInstance<Action>();

            Assert.AreEqual("Action ID: 2 status is 4.", _action.Process(actionDo).Exception.InnerException.Message);
        }

        [Test]
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

        [Test]
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
		  [Test]
		  public void GetUpstreamActivities_ActionDOIsNull_ExpectedArgumentNullException()
		  {
			  var ex = Assert.Throws<ArgumentNullException>(() => _action.GetUpstreamActivities(null));
			  Assert.AreEqual("actionDO", ex.ParamName);
		  }
		  [Test]
		  public void GetUpstreamActivities_1Level_ShoudBeOk()
		  {
			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1 };
			  ActionDO l1_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ActionList = l1_aList;

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  uow.ActionRepository.Add(l1_a1);
				  uow.ActionRepository.Add(l1_a2);
				  uow.ActionListRepository.Add(l1_aList);

				  uow.SaveChanges();
			  }
			  var upstreamActivities = _action.GetUpstreamActivities(l1_a2);

			  Assert.AreEqual(2, upstreamActivities.Count);
			  Assert.AreEqual(l1_a1, upstreamActivities[0]);
			  Assert.AreEqual(l1_aList, upstreamActivities[1]);
		  }
		  [Test]
		  public void GetUpstreamActivities_2Levels_ShoudBeOk()
		  {
			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1 };
			  ActionDO l2_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Actions.Add(l2_a1);
			  l2_a1.ActionList = l2_aList;
			  l2_aList.Actions.Add(l2_a2);
			  l2_a2.ActionList = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1 };
			  ActionDO l1_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ActionList = l1_aList;

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  uow.ActionRepository.Add(l2_a1);
				  uow.ActionRepository.Add(l2_a2);
				  uow.ActionListRepository.Add(l2_aList);

				  uow.ActionRepository.Add(l1_a1);
				  uow.ActionRepository.Add(l1_a2);
				  uow.ActionListRepository.Add(l1_aList);

				  uow.SaveChanges();
			  }
			  var upstreamActivities = _action.GetUpstreamActivities(l2_a1);

			  Assert.AreEqual(4, upstreamActivities.Count);
			  Assert.AreEqual(l2_a1, upstreamActivities[0]);
			  Assert.AreEqual(l2_aList, upstreamActivities[1]);
			  Assert.AreEqual(l1_a1, upstreamActivities[2]);
			  Assert.AreEqual(l1_aList, upstreamActivities[3]);
		  }
		  [Test]
		  public void GetUpstreamActivities_3Levels_ShoudBeOk()
		  {
			  // Level 3
			  ActionDO l3_a1 = new ActionDO() { Id = 1, };
			  ActionDO l3_a2 = new ActionDO() { Id = 2, };
			  ActionDO l3_a3 = new ActionDO() { Id = 3, };
			  ActionListDO l3_aList = new ActionListDO() { Ordering = 3 , ActionListType = ActionListType.Immediate};
			  l3_aList.Actions.Add(l3_a1);
			  l3_a1.ActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a2);
			  l3_a2.ActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a3);
			  l3_a3.ActionList = l3_aList;

			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1 };
			  ActionDO l2_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Actions.Add(l2_a1);
			  l2_a1.ActionList = l2_aList;
			  l2_aList.Actions.Add(l2_a2);
			  l2_a2.ActionList = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1 };
			  ActionDO l1_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ActionList = l1_aList;

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  uow.ActionRepository.Add(l3_a1);
				  uow.ActionRepository.Add(l3_a2);
				  uow.ActionRepository.Add(l3_a3);
				  uow.ActionListRepository.Add(l3_aList);

				  uow.ActionRepository.Add(l2_a1);
				  uow.ActionRepository.Add(l2_a2);
				  uow.ActionListRepository.Add(l2_aList);

				  uow.ActionRepository.Add(l1_a1);
				  uow.ActionRepository.Add(l1_a2);
				  uow.ActionListRepository.Add(l1_aList);

				  uow.SaveChanges();
			  }
			  var upstreamActivities = _action.GetUpstreamActivities(l3_a3);

			  Assert.AreEqual(6, upstreamActivities.Count);
			  Assert.AreEqual(l3_a1, upstreamActivities[0]);
			  Assert.AreEqual(l3_aList, upstreamActivities[1]);
			  Assert.AreEqual(l2_a1, upstreamActivities[2]);
			  Assert.AreEqual(l2_aList, upstreamActivities[3]);
			  Assert.AreEqual(l1_a1, upstreamActivities[4]);
			  Assert.AreEqual(l1_aList, upstreamActivities[5]);
		  }
		  [Test]
		  public void GetUpstreamActivities_4Levels_ShoudBeOk()
		  {
			  // Level 4
			  ActionDO l4_a1 = new ActionDO() { Id = 1, };
			  ActionDO l4_a2 = new ActionDO() { Id = 2, };
			  ActionDO l4_a3 = new ActionDO() { Id = 3, };
			  ActionListDO l4_aList = new ActionListDO() { Ordering = 4, ActionListType = ActionListType.Immediate };
			  l4_aList.Actions.Add(l4_a1);
			  l4_a1.ActionList = l4_aList;
			  l4_aList.Actions.Add(l4_a2);
			  l4_a2.ActionList = l4_aList;
			  l4_aList.Actions.Add(l4_a3);
			  l4_a3.ActionList = l4_aList;

			  // Level 3
			  ActionDO l3_a1 = new ActionDO() { Id = 1, };
			  ActionDO l3_a2 = new ActionDO() { Id = 2, };
			  ActionDO l3_a3 = new ActionDO() { Id = 3, };
			  ActionListDO l3_aList = new ActionListDO() { Ordering = 3, ActionListType = ActionListType.Immediate };
			  l3_aList.Actions.Add(l3_a1);
			  l3_a1.ActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a2);
			  l3_a2.ActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a3);
			  l3_a3.ActionList = l3_aList;

			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1 };
			  ActionDO l2_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Actions.Add(l2_a1);
			  l2_a1.ActionList = l2_aList;
			  l2_aList.Actions.Add(l2_a2);
			  l2_a2.ActionList = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1 };
			  ActionDO l1_a2 = new ActionDO() { Id = 2 };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ActionList = l1_aList;

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  uow.ActionRepository.Add(l3_a1);
				  uow.ActionRepository.Add(l3_a2);
				  uow.ActionRepository.Add(l3_a3);
				  uow.ActionListRepository.Add(l3_aList);

				  uow.ActionRepository.Add(l2_a1);
				  uow.ActionRepository.Add(l2_a2);
				  uow.ActionListRepository.Add(l2_aList);

				  uow.ActionRepository.Add(l1_a1);
				  uow.ActionRepository.Add(l1_a2);
				  uow.ActionListRepository.Add(l1_aList);

				  uow.SaveChanges();
			  }
			  var upstreamActivities = _action.GetUpstreamActivities(l4_a3);

			  Assert.AreEqual(8, upstreamActivities.Count);
			  Assert.AreEqual(l4_a1, upstreamActivities[0]);
			  Assert.AreEqual(l4_aList, upstreamActivities[1]);
			  Assert.AreEqual(l3_a1, upstreamActivities[2]);
			  Assert.AreEqual(l3_aList, upstreamActivities[3]);
			  Assert.AreEqual(l2_a1, upstreamActivities[4]);
			  Assert.AreEqual(l2_aList, upstreamActivities[5]);
			  Assert.AreEqual(l1_a1, upstreamActivities[6]);
			  Assert.AreEqual(l1_aList, upstreamActivities[7]);
		  }

        [Test]
        public void Process_ActionUnstarted_ShouldBeCompleted()
        {
            ActionDO actionDo = FixtureData.TestActionUnstarted();
            Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();
            var response = _action.Process(actionDo);
            Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test]
        public void Dispatch_PayloadDTO_ShouldBeDispatched()
        {
            ActionDO actionDo = FixtureData.TestActionUnstarted();
            Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();
            var pluginRegistration = BasePluginRegistration.GetPluginType(actionDo);
            Uri baseUri = new Uri(pluginRegistration.BaseUrl, UriKind.Absolute);
            var response = _action.Dispatch(actionDo, baseUri);
            Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));

        }

        [Test]
        public void GetAvailableActions_ReturnsActionsForAccount()
        {
            const string unavailablePluginName = "UnavailablePlugin";
            const string noAccessPluginName = "NoAccessPlugin";
            const string userAccessPluginName = "AvailableWithUserAccessPlugin";
            const string adminAccessPluginName = "AvailableWithAdminAccessPlugin";
            var unavailablePluginRegistration = new Mock<IPluginRegistration>();
            var noAccessPluginRegistration = new Mock<IPluginRegistration>();
            var userAccessPluginRegistration = new Mock<IPluginRegistration>();
            var adminAccessPluginRegistration = new Mock<IPluginRegistration>();
            ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(unavailablePluginRegistration.Object).Named(unavailablePluginName));
            ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(noAccessPluginRegistration.Object).Named(noAccessPluginName));
            ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(userAccessPluginRegistration.Object).Named(userAccessPluginName));
            ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(adminAccessPluginRegistration.Object).Named(adminAccessPluginName));
            var account = new DockyardAccountDO()
            {
                Subscriptions = new List<SubscriptionDO>()
                {
                    new SubscriptionDO()
                    {
                        AccessLevel = AccessLevel.None,
                        Plugin = new PluginDO() {Name = noAccessPluginName}
                    },
                    new SubscriptionDO()
                    {
                        AccessLevel = AccessLevel.User,
                        Plugin = new PluginDO() {Name = userAccessPluginName}
                    },
                    new SubscriptionDO()
                    {
                        AccessLevel = AccessLevel.Admin,
                        Plugin = new PluginDO() {Name = adminAccessPluginName}
                    },
                }
            };

            Core.Services.Action _action = ObjectFactory.GetInstance<Core.Services.Action>();
            List<ActionTemplateDO> curActionTemplateDO = _action.GetAvailableActions(account).ToList();

            //Assert
            Assert.AreEqual(4, curActionTemplateDO.Count);
            Assert.That(curActionTemplateDO, Is.Ordered.By("ActionType"));


        }

    }
}
