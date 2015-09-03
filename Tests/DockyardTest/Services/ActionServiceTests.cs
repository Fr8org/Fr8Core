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
            // Test.
            Action action = new Action();
            var processTemplate = FixtureData.TestProcessTemplate2();
            var payloadMappings = FixtureData.FieldMappings;
            var actionDo = FixtureData.IntegrationTestAction();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.ActionRepository.Add(actionDo);
                uow.ActionListRepository.Add(actionDo.ParentActionList);
                uow.ProcessRepository.Add(actionDo.ParentActionList.Process);
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
                uow.ActionListRepository.Add(actionDo.ParentActionList);
                uow.ProcessRepository.Add(actionDo.ParentActionList.Process);
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
			  var actionTempate = new ActionTemplateDO()
            {
                Id = 1,
                ActionType = "Test action",
                ParentPluginRegistration = "Test registration",
                Version = "1"
            };
			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ParentActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ParentActionList = l1_aList;

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
			  var actionTempate = new ActionTemplateDO()
			  {
				  Id = 1,
				  ActionType = "Test action",
				  ParentPluginRegistration = "Test registration",
				  Version = "1"
			  };
			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Actions.Add(l2_a1);
			  l2_a1.ParentActionList = l2_aList;
			  l2_aList.Actions.Add(l2_a2);
			  l2_a2.ParentActionList = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ParentActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ParentActionList = l1_aList;

			  l2_aList.ParentActionList = l1_aList;

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
			  var actionTempate = new ActionTemplateDO()
			  {
				  Id = 1,
				  ActionType = "Test action",
				  ParentPluginRegistration = "Test registration",
				  Version = "1"
			  };
			  // Level 3
			  ActionDO l3_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l3_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionDO l3_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			  ActionListDO l3_aList = new ActionListDO() { Ordering = 3 , ActionListType = ActionListType.Immediate};
			  l3_aList.Actions.Add(l3_a1);
			  l3_a1.ParentActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a2);
			  l3_a2.ParentActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a3);
			  l3_a3.ParentActionList = l3_aList;

			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Actions.Add(l2_a1);
			  l2_a1.ParentActionList = l2_aList;
			  l2_aList.Actions.Add(l2_a2);
			  l2_a2.ParentActionList = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ParentActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ParentActionList = l1_aList;

			  l3_aList.ParentActionList = l2_aList;
			  l2_aList.ParentActionList = l1_aList;

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
			  var actionTempate = new ActionTemplateDO()
			  {
				  Id = 1,
				  ActionType = "Test action",
				  ParentPluginRegistration = "Test registration",
				  Version = "1"
			  };
			  // Level 4
			  ActionDO l4_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l4_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionDO l4_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			  ActionListDO l4_aList = new ActionListDO() { Ordering = 4, ActionListType = ActionListType.Immediate };
			  l4_aList.Actions.Add(l4_a1);
			  l4_a1.ParentActionList = l4_aList;
			  l4_aList.Actions.Add(l4_a2);
			  l4_a2.ParentActionList = l4_aList;
			  l4_aList.Actions.Add(l4_a3);
			  l4_a3.ParentActionList = l4_aList;

			  // Level 3
			  ActionDO l3_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l3_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionDO l3_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			  ActionListDO l3_aList = new ActionListDO() { Ordering = 3, ActionListType = ActionListType.Immediate };
			  l3_aList.Actions.Add(l3_a1);
			  l3_a1.ParentActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a2);
			  l3_a2.ParentActionList = l3_aList;
			  l3_aList.Actions.Add(l3_a3);
			  l3_a3.ParentActionList = l3_aList;

			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Actions.Add(l2_a1);
			  l2_a1.ParentActionList = l2_aList;
			  l2_aList.Actions.Add(l2_a2);
			  l2_a2.ParentActionList = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ParentActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ParentActionList = l1_aList;

			  l4_aList.ParentActionList = l3_aList;
			  l3_aList.ParentActionList = l2_aList;
			  l2_aList.ParentActionList = l1_aList;

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

        //this test is being phased out
        [Test,Ignore]
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
		  [Test]
		  public void GetDownstreamActivities_1Levels_ShoudBeOk()
		  {
			  var actionTempate = new ActionTemplateDO()
			  {
				  Id = 1,
				  ActionType = "Test action",
				  ParentPluginRegistration = "Test registration",
				  Version = "1"
			  };
			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate, Ordering = 1 };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate, Ordering = 2 };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Actions.Add(l1_a1);
			  l1_a1.ParentActionList = l1_aList;
			  l1_aList.Actions.Add(l1_a2);
			  l1_a2.ParentActionList = l1_aList;

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  uow.ActionRepository.Add(l1_a1);
				  uow.ActionRepository.Add(l1_a2);
				  uow.ActionListRepository.Add(l1_aList);

				  uow.SaveChanges();
			  }
			  var downstreamActivities = _action.GetDownstreamActivities(l1_a1);

			  Assert.AreEqual(1, downstreamActivities.Count);
			  Assert.AreEqual(l1_a2, downstreamActivities[0]);
		  }
		  [Test, Ignore]
		  public void GetDownstreamActivities_4Levels_ShoudBeOk()
		  {
			  var actionTempate = new ActionTemplateDO()
			  {
				  Id = 1,
				  ActionType = "Test action",
				  ParentPluginRegistration = "Test registration",
				  Version = "1"
			  };
			  ActionListDO al_1 = new ActionListDO() {  Id = 1, Ordering = 1, ActionListType = ActionListType.Immediate, Name = "Fly To Kiev"};
			  ActionDO a_23 = new ActionDO() { Id = 23, ActionTemplate = actionTempate, Name = "Drive to  Ariport"};
			  al_1.Actions.Add(a_23);
			  a_23.ParentActionList = al_1;

			  ActionListDO al_43 = new ActionListDO() {  Id = 43, Ordering = 2, ActionListType = ActionListType.Immediate, Name = "Board Plane"};
			  al_43.ParentActionList = al_1;
			  ActionDO a_44 = new ActionDO() { Id = 44, Ordering = 1, ActionTemplate = actionTempate, Name = "Check Baggage" };
			  a_44.ParentActionList = al_43;
			  al_43.Actions.Add(a_44);
			  ActionDO a_46 = new ActionDO() { Id = 46, Ordering = 2, ActionTemplate = actionTempate, Name = "Buy Ticket" };
			  a_46.ParentActionList = al_43;
			  al_43.Actions.Add(a_46);
			  ActionDO a_48 = new ActionDO() { Id = 48, Ordering = 3, ActionTemplate = actionTempate, Name = "Get on Plane" };
			  a_48.ParentActionList = al_43;
			  al_43.Actions.Add(a_48);

			  ActionListDO al_52 = new ActionListDO() { Id = 52, Ordering = 3, ActionListType = ActionListType.Immediate, Name = "BLA BLA" };
			  ActionDO a_53 = new ActionDO() { Id = 53, Ordering = 1, ActionTemplate = actionTempate, Name = "A1" };
			  a_53.ParentActionList = al_52;
			  al_52.Actions.Add(a_53);

			  ActionListDO al_54 = new ActionListDO() { Id = 54, Ordering = 2, ActionListType = ActionListType.Immediate, Name = "AL2" };
			  ActionDO a_56 = new ActionDO() { Id = 56, Ordering = 1, ActionTemplate = actionTempate, Name = "A11" };
			  a_56.ParentActionList = al_54;
			  al_54.Actions.Add(a_56);
			  ActionDO a_57 = new ActionDO() { Id = 57, Ordering = 2, ActionTemplate = actionTempate, Name = "A22" };
			  a_57.ParentActionList = al_54;
			  al_54.Actions.Add(a_57);
			  ActionDO a_58 = new ActionDO() { Id = 58, Ordering = 3, ActionTemplate = actionTempate, Name = "A33" };
			  a_58.ParentActionList = al_54;
			  al_54.Actions.Add(a_58);

			  ActionDO a_55 = new ActionDO() { Id = 55, Ordering = 3, ActionTemplate = actionTempate, Name = "A3" };
			  a_55.ParentActionList = al_52;
			  al_52.Actions.Add(a_55);

			  ActionListDO al_59 = new ActionListDO() { Id = 59, Ordering = 4, ActionListType = ActionListType.Immediate, Name = "BLA BLA2" };
			  ActionDO a_60 = new ActionDO() { Id = 60, Ordering = 1, ActionTemplate = actionTempate, Name = "A1" };
			  a_60.ParentActionList = al_59;
			  al_59.Actions.Add(a_60);
			 
			  ActionListDO al_61 = new ActionListDO() { Id = 61, Ordering = 2, ActionListType = ActionListType.Immediate, Name = "AL2" };
			  ActionDO a_63 = new ActionDO() { Id = 63, Ordering = 1, ActionTemplate = actionTempate, Name = "A11" };
			  a_63.ParentActionList = al_61;
			  al_61.Actions.Add(a_63);
			  ActionDO a_64 = new ActionDO() { Id = 64, Ordering = 2, ActionTemplate = actionTempate, Name = "A22" };
			  a_64.ParentActionList = al_61;
			  al_61.Actions.Add(a_64);
			  ActionDO a_65 = new ActionDO() { Id = 65, Ordering = 3, ActionTemplate = actionTempate, Name = "A33" };
			  a_65.ParentActionList = al_61;
			  al_61.Actions.Add(a_65);

			  ActionDO a_62 = new ActionDO() { Id = 62, Ordering = 3, ActionTemplate = actionTempate, Name = "A3" };
			  a_62.ParentActionList = al_59;
			  al_59.Actions.Add(a_62);

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  uow.ActionRepository.Add(a_23);
				  uow.ActionRepository.Add(a_44);
				  uow.ActionRepository.Add(a_46);
				  uow.ActionRepository.Add(a_48);
				  uow.ActionRepository.Add(a_53);
				  uow.ActionRepository.Add(a_55);
				  uow.ActionRepository.Add(a_56);
				  uow.ActionRepository.Add(a_57);
				  uow.ActionRepository.Add(a_58);
				  uow.ActionRepository.Add(a_60);
				  uow.ActionRepository.Add(a_62);
				  uow.ActionRepository.Add(a_63);
				  uow.ActionRepository.Add(a_64);
				  uow.ActionRepository.Add(a_65);

				  uow.ActionListRepository.Add(al_43);
				  uow.ActionListRepository.Add(al_52);
				  uow.ActionListRepository.Add(al_54);
				  uow.ActionListRepository.Add(al_59);
				  uow.ActionListRepository.Add(al_61);

				  uow.SaveChanges();
			  }
			  var downstreamActivities = _action.GetDownstreamActivities(a_46);

			  Assert.AreEqual(8, downstreamActivities.Count);
			  
		  }
    }
}
