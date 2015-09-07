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
using Utilities;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class ActionServiceTests : BaseTest
    {
        private IAction _action;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActionTemplateDO> _pr1Actions = new List<ActionTemplateDO>() { new ActionTemplateDO() { Name = "Write", Version = "1.0" }, new ActionTemplateDO() { Name = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActionTemplateDO> _pr2Actions = new List<ActionTemplateDO>() { new ActionTemplateDO() { Name = "SQL Write", Version = "1.0" }, new ActionTemplateDO() { Name = "SQL Read", Version = "1.0" } };

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
            var curActionDO = FixtureData.TestAction22();
            string curJsonResult = _action.GetConfigurationSettings(curActionDO);
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
			  Assert.AreEqual("curActionDO", ex.ParamName);
		  }
		  [Test]
		  public void GetUpstreamActivities_1Level_ShoudBeOk()
		  {
			  var actionTempate = new ActionTemplateDO()
            {
                Id = 1,
                Version = "1"
            };
			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Activities.Add(l1_a1);
			  l1_a1.ParentActivity = l1_aList;
			  l1_aList.Activities.Add(l1_a2);
			  l1_a2.ParentActivity = l1_aList;

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
				  Version = "1"
			  };
			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Activities.Add(l2_a1);
			  l2_a1.ParentActivity = l2_aList;
			  l2_aList.Activities.Add(l2_a2);
			  l2_a2.ParentActivity = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Activities.Add(l1_a1);
			  l1_a1.ParentActivity = l1_aList;
			  l1_aList.Activities.Add(l1_a2);
			  l1_a2.ParentActivity = l1_aList;

			  l2_aList.ParentActivity = l1_aList;

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
				  Version = "1"
			  };
			  // Level 3
			  ActionDO l3_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l3_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionDO l3_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			  ActionListDO l3_aList = new ActionListDO() { Ordering = 3 , ActionListType = ActionListType.Immediate};
			  l3_aList.Activities.Add(l3_a1);
			  l3_a1.ParentActivity = l3_aList;
			  l3_aList.Activities.Add(l3_a2);
			  l3_a2.ParentActivity = l3_aList;
			  l3_aList.Activities.Add(l3_a3);
			  l3_a3.ParentActivity = l3_aList;

			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Activities.Add(l2_a1);
			  l2_a1.ParentActivity = l2_aList;
			  l2_aList.Activities.Add(l2_a2);
			  l2_a2.ParentActivity = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Activities.Add(l1_a1);
			  l1_a1.ParentActivity = l1_aList;
			  l1_aList.Activities.Add(l1_a2);
			  l1_a2.ParentActivity = l1_aList;

			  l3_aList.ParentActivity = l2_aList;
			  l2_aList.ParentActivity = l1_aList;

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
				  Version = "1"
			  };
			  // Level 4
			  ActionDO l4_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l4_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionDO l4_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			  ActionListDO l4_aList = new ActionListDO() { Ordering = 4, ActionListType = ActionListType.Immediate };
			  l4_aList.Activities.Add(l4_a1);
			  l4_a1.ParentActivity = l4_aList;
			  l4_aList.Activities.Add(l4_a2);
			  l4_a2.ParentActivity = l4_aList;
			  l4_aList.Activities.Add(l4_a3);
			  l4_a3.ParentActivity = l4_aList;

			  // Level 3
			  ActionDO l3_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l3_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionDO l3_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			  ActionListDO l3_aList = new ActionListDO() { Ordering = 3, ActionListType = ActionListType.Immediate };
			  l3_aList.Activities.Add(l3_a1);
			  l3_a1.ParentActivity = l3_aList;
			  l3_aList.Activities.Add(l3_a2);
			  l3_a2.ParentActivity = l3_aList;
			  l3_aList.Activities.Add(l3_a3);
			  l3_a3.ParentActivity = l3_aList;

			  // Level 2
			  ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			  l2_aList.Activities.Add(l2_a1);
			  l2_a1.ParentActivity = l2_aList;
			  l2_aList.Activities.Add(l2_a2);
			  l2_a2.ParentActivity = l2_aList;

			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Activities.Add(l1_a1);
			  l1_a1.ParentActivity = l1_aList;
			  l1_aList.Activities.Add(l1_a2);
			  l1_a2.ParentActivity = l1_aList;

			  l4_aList.ParentActivity = l3_aList;
			  l3_aList.ParentActivity = l2_aList;
			  l2_aList.ParentActivity = l1_aList;

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
				  Version = "1"
			  };
			  // Level 1
			  ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate, Ordering = 1 };
			  ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate, Ordering = 2 };
			  ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			  l1_aList.Activities.Add(l1_a1);
			  l1_a1.ParentActivity = l1_aList;
			  l1_aList.Activities.Add(l1_a2);
			  l1_a2.ParentActivity = l1_aList;

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
		  [Test(Description="Big tree from https://maginot.atlassian.net/wiki/display/SH/Getting+Upstream+and+Downstream+Activities+Lists")]
		  public void GetDownstreamActivities_BigTreeFromWikiPageWithActivity46_ShoudBeOk()
		  {
			  List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  var w = uow.ActivityRepository.GetAll().ToList();


				  foreach (var actionList in actionLists)
				  {
					  //foreach (var action in actionList.Activities)
					  //	uow.ActivityRepository.Add(action);
					  uow.ActivityRepository.Add(actionList);
				  }
				  uow.SaveChanges();
				  w = uow.ActivityRepository.GetAll().ToList();
				  var actionWithId46 = uow.ActionRepository.GetByKey(46);
				  var downstreamActivities = _action.GetDownstreamActivities(actionWithId46);

				  Assert.AreEqual(15, downstreamActivities.Count);
				  Assert.AreEqual(true, downstreamActivities[0] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[0].GetType().Name));
				  Assert.AreEqual(48, downstreamActivities[0].Id, "Expected Action with Id '{0}' but got '{1}".format(48, downstreamActivities[0].Id));
				  Assert.AreEqual(true, downstreamActivities[1] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[1].GetType().Name));
				  Assert.AreEqual(52, downstreamActivities[1].Id, "Expected ActionListDO with Id '{0}' but got '{1}".format(52, downstreamActivities[1].Id));
				  Assert.AreEqual(true, downstreamActivities[2] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[2].GetType().Name));
				  Assert.AreEqual(53, downstreamActivities[2].Id, "Expected Action with Id '{0}' but got '{1}".format(53, downstreamActivities[2].Id));
				  Assert.AreEqual(true, downstreamActivities[3] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[3].GetType().Name));
				  Assert.AreEqual(54, downstreamActivities[3].Id, "Expected ActionListDO with Id '{0}' but got '{1}".format(54, downstreamActivities[3].Id));
				  Assert.AreEqual(true, downstreamActivities[4] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[4].GetType().Name));
				  Assert.AreEqual(56, downstreamActivities[4].Id, "Expected Action with Id '{0}' but got '{1}".format(56, downstreamActivities[4].Id));
				  Assert.AreEqual(true, downstreamActivities[5] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[5].GetType().Name));
				  Assert.AreEqual(57, downstreamActivities[5].Id, "Expected Action with Id '{0}' but got '{1}".format(57, downstreamActivities[5].Id));
				  Assert.AreEqual(true, downstreamActivities[6] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[6].GetType().Name));
				  Assert.AreEqual(58, downstreamActivities[6].Id, "Expected Action with Id '{0}' but got '{1}".format(58, downstreamActivities[6].Id));
				  Assert.AreEqual(true, downstreamActivities[7] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[7].GetType().Name));
				  Assert.AreEqual(55, downstreamActivities[7].Id, "Expected Action with Id '{0}' but got '{1}".format(55, downstreamActivities[7].Id));
				  Assert.AreEqual(true, downstreamActivities[8] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[8].GetType().Name));
				  Assert.AreEqual(59, downstreamActivities[8].Id, "Expected ActionListDO with Id '{0}' but got '{1}".format(59, downstreamActivities[8].Id));
				  Assert.AreEqual(true, downstreamActivities[9] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[9].GetType().Name));
				  Assert.AreEqual(60, downstreamActivities[9].Id, "Expected Action with Id '{0}' but got '{1}".format(60, downstreamActivities[9].Id));
				  Assert.AreEqual(true, downstreamActivities[10] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[10].GetType().Name));
				  Assert.AreEqual(61, downstreamActivities[10].Id, "Expected ActionListDO with Id '{0}' but got '{1}".format(61, downstreamActivities[10].Id));
				  Assert.AreEqual(true, downstreamActivities[11] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[11].GetType().Name));
				  Assert.AreEqual(63, downstreamActivities[11].Id, "Expected Action with Id '{0}' but got '{1}".format(63, downstreamActivities[11].Id));
				  Assert.AreEqual(true, downstreamActivities[12] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[12].GetType().Name));
				  Assert.AreEqual(64, downstreamActivities[12].Id, "Expected Action with Id '{0}' but got '{1}".format(64, downstreamActivities[12].Id));
				  Assert.AreEqual(true, downstreamActivities[13] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[13].GetType().Name));
				  Assert.AreEqual(65, downstreamActivities[13].Id, "Expected Action with Id '{0}' but got '{1}".format(65, downstreamActivities[13].Id));
				  Assert.AreEqual(true, downstreamActivities[14] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[14].GetType().Name));
				  Assert.AreEqual(62, downstreamActivities[14].Id, "Expected Action with Id '{0}' but got '{1}".format(62, downstreamActivities[14].Id));
			  }
		  }
		  [Test]
		  public void GetUpstreamActivities_BigTreeFromWikiPageWithActivity46_ShoudBeOk()
		  {
			  List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();

			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			  {
				  foreach (var actionList in actionLists)
				  {
					  foreach (var action in actionList.Activities)
						  uow.ActivityRepository.Add(action);
					  uow.ActivityRepository.Add(actionList);
				  }
				  uow.SaveChanges();

				  var actionWithId46 = uow.ActionRepository.GetByKey(46);
				  var downstreamActivities = _action.GetUpstreamActivities(actionWithId46);

				  Assert.AreEqual(4, downstreamActivities.Count);
				  Assert.AreEqual(true, downstreamActivities[0] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[0].GetType().Name));
				  Assert.AreEqual(44, downstreamActivities[0].Id, "Expected Action with Id '{0}' but got '{1}".format(44, downstreamActivities[0].Id));
				  Assert.AreEqual(true, downstreamActivities[1] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[1].GetType().Name));
				  Assert.AreEqual(43, downstreamActivities[1].Id, "Expected ActionListDO with Id '{0}' but got '{1}".format(43, downstreamActivities[1].Id));
				  Assert.AreEqual(true, downstreamActivities[2] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[2].GetType().Name));
				  Assert.AreEqual(23, downstreamActivities[2].Id, "Expected Action with Id '{0}' but got '{1}".format(23, downstreamActivities[2].Id));
				  Assert.AreEqual(true, downstreamActivities[3] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[3].GetType().Name));
				  Assert.AreEqual(1, downstreamActivities[3].Id, "Expected ActionListDO with Id '{0}' but got '{1}".format(1, downstreamActivities[3].Id));
			  }
		  }
		  
    }
}
