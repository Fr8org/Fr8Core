using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Terminal;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;
using Newtonsoft.Json.Linq;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Action = Hub.Services.Activity;


namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class ActionServiceTests : BaseTest
    {
        private IActivity _activity;
        private ICrateManager _crate;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActivityTemplateDO> _pr1Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActivityTemplateDO> _pr2Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "SQL Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "SQL Read", Version = "1.0" } };
        private bool _eventReceived;
        private BaseTerminalActivity _baseTerminalAction;
        private ITerminal _terminal;
        private Mock<ITerminalTransmitter> TerminalTransmitterMock
        {
            get { return Mock.Get(ObjectFactory.GetInstance<ITerminalTransmitter>()); }
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();

            _activity = ObjectFactory.GetInstance<IActivity>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData(_uow);
            _eventReceived = false;
            _baseTerminalAction = new BaseTerminalActivity();
            _terminal = ObjectFactory.GetInstance<Terminal>();
        }
        
        // DO-1214
//        [Test]
//        public async void Action_Configure_ExistingActionShouldBeUpdatedWithNewAction()
//        {
//            //Arrange
//            ActionDO curActivityDO = FixtureData.IntegrationTestAction();
//            UpdateDatabase(curActivityDO);
//
//            ActionDTO actionDto = Mapper.Map<ActionDTO>(curActivityDO);
//
//            //set the new name
//            actionDto.Name = "NewActionFromServer";
//            PluginTransmitterMock.Setup(rc => rc.CallActionAsync<ActionDTO>(It.IsAny<string>(), It.IsAny<ActionDTO>()))
//                .Returns(() => Task.FromResult(actionDto));
//
//            //Act
//            var returnedAction = await _action.Configure(curActivityDO);
//
//            //Assert
//            //get the action from the database
//            var updatedActionDO = _uow.ActionRepository.GetByKey(returnedAction.Id);
//            Assert.IsNotNull(updatedActionDO);
//            Assert.AreEqual(updatedActionDO.Name, actionDto.Name);
//        }

        // DO-1214
//        [Test]
//        public void UpdateCurrentActivity_ShouldUpdateCurrentActivity()
//        {
//            var curActionList = FixtureData.TestActionList2();
//
//            // Set current activity
//            curActionList.CurrentActivity = curActionList.Activities.Single(a => a.Id == 1);
//            curActionList.Id = curActionList.CurrentActivity.Id;
//
//            Assert.AreEqual(1, curActionList.CurrentActivity.Id);
//
//            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                uow.ActionListRepository.Add(curActionList);
//                uow.SaveChanges();
//
//                _action.UpdateCurrentActivity(curActionList.CurrentActivityID.Value, uow);
//
//                Assert.AreEqual(2, curActionList.CurrentActivity.Id);
//
//                // Check when current action is the only action in action list (should set null)
//                curActionList.Activities.RemoveAt(1);
//                uow.SaveChanges();
//
//                _action.UpdateCurrentActivity(curActionList.CurrentActivityID.Value, uow);
//                Assert.AreEqual(null, curActionList.CurrentActivity);
//            }
//        }




        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async void Action_Configure_WithNullActionTemplate_ThrowsArgumentNullException()
        {
            var _service = new Action();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _service.Configure(uow, null, null);
            }
        }

        [Test]
        public void CanCRUDActions()
        {
            ActivityDO origActivityDO;

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute1();
                uow.PlanRepository.Add(plan);

                var subroute = FixtureData.TestSubrouteDO1();
                plan.ChildNodes.Add(subroute);

                origActivityDO = new FixtureData(uow).TestActivity3();

                origActivityDO.ParentRouteNodeId = subroute.Id;

                uow.ActivityTemplateRepository.Add(origActivityDO.ActivityTemplate);
                uow.SaveChanges();
            }

            IActivity activity = new Activity();

            //Add
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activity.SaveOrUpdateActivity(uow, origActivityDO);
            }

            ActivityDO activityDO;
            //Get
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activityDO = activity.GetById(uow, origActivityDO.Id);
            }

            Assert.AreEqual(origActivityDO.Id, activityDO.Id);
            Assert.AreEqual(origActivityDO.CrateStorage, activityDO.CrateStorage);

            Assert.AreEqual(origActivityDO.Ordering, activityDO.Ordering);

            ISubroute subRoute = new Subroute();
            //Delete
            subRoute.DeleteActivity(null, activityDO.Id, true);
        }

        [Test]
        public void ActionWithNestedUpdated_StructureUnchanged()
        {
            var tree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();
            var updatedTree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO
                {
                    RouteState = RouteState.Active,
                    Name = "name",
                    ChildNodes = { tree }
                };
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();

                Visit(updatedTree, x => x.Label = string.Format("We were here {0}", x.Id));

                _activity.SaveOrUpdateActivity(uow, updatedTree);

                var result = uow.PlanRepository.GetById<ActivityDO>(tree.Id);
                Compare(updatedTree, result, (r, a) =>
                {
                    if (r.Label != a.Label)
                    {
                        throw new Exception("Update failed");
                    }
                });
            }
        }

        [Test]
        public void ActionWithNestedUpdated_RemoveElements()
        {
            var tree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();
            var updatedTree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO
                {
                    RouteState = RouteState.Active,
                    Name = "name",
                    ChildNodes = { tree }
                };
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
                int removeCounter = 0;

                Visit(updatedTree, a =>
                {
                    if (removeCounter % 3 == 0 && a.ParentRouteNode != null)
                    {
                        a.ParentRouteNode.ChildNodes.Remove(a);
                    }

                    removeCounter++;
                });

                
                _activity.SaveOrUpdateActivity(uow, updatedTree);

                var result = uow.PlanRepository.GetById<ActivityDO>(tree.Id);
                Compare(updatedTree, result, (r, a) =>
                {
                    if (r.Id != a.Id)
                    {
                        throw new Exception("Update failed");
                    }
                });
            }
        }

        [Test]
        public void ActionWithNestedUpdated_AddElements()
        {
            var tree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();
            var updatedTree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO
                {
                    Name = "name",
                    RouteState = RouteState.Active,
                    ChildNodes = { tree }
                };

                uow.PlanRepository.Add(plan);
                uow.SaveChanges();

                int addCounter = 0;

                Visit(updatedTree, a =>
                {
                    if (addCounter % 3 == 0 && a.ParentRouteNode != null)
                    {
                        var newAction = new ActivityDO
                        {
                            Id = FixtureData.GetTestGuidById(addCounter + 666),
                            ParentRouteNode = a,
                        };

                        a.ParentRouteNode.ChildNodes.Add(newAction);
                    }

                    addCounter++;
                });

                for (int i = 0; i < 4; i++)
                {
                    Visit(updatedTree, a =>
                    {
                        // if (a.Id > 666)
                        if (FixtureData.GetTestIdByGuid(a.Id) > 666)
                        {
                            var newAction = new ActivityDO
                            {
                                Id = FixtureData.GetTestGuidById(addCounter + 666),
                                ParentRouteNode = a,
                            };

                            a.ParentRouteNode.ChildNodes.Add(newAction);
                        }

                        addCounter++;
                    });
                }

                updatedTree.ParentRouteNodeId = plan.Id;

                _activity.SaveOrUpdateActivity(uow, updatedTree);

                var result = uow.PlanRepository.GetById<ActivityDO>(tree.Id);
                Compare(updatedTree, result, (r, a) =>
                {
                    if (r.Id != a.Id)
                    {
                        throw new Exception("Update failed");
                    }
                });
            }
        }

//        [Test]
//        public void CreateNewAction()
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                var terminal = new TerminalDO()
//                {
//                    TerminalStatus = TerminalStatus.Active,
//                    Endpoint = "ep",
//                    Version = "1",
//                    Name = "Terminal",
//                    Secret = Guid.NewGuid().ToString()
//                };
//
//                uow.TerminalRepository.Add(terminal);
//                uow.SaveChanges();
//
//                var template = new ActivityTemplateDO("Template1", "label", "1", "description", terminal.Id);
//                uow.ActivityTemplateRepository.Add(template);
//                var parent = new ActivityDO();
//                uow.ActivityRepository.Add(parent);
//
//                uow.SaveChanges();
//
//                const string actionName = "TestAction";
//                var response = _activity.Create(uow, template.Id, actionName, null, parent);
//
//                Assert.AreEqual(parent.ChildNodes.Count, 1);
//                Assert.AreEqual(parent.ChildNodes[0], response);
//                Assert.AreEqual(response.Name, actionName);
//            }
//        }

        private void Compare(ActivityDO reference, ActivityDO actual, Action<ActivityDO, ActivityDO> callback)
        {
            callback(reference, actual);

            if (reference.ChildNodes.Count != actual.ChildNodes.Count)
            {
                throw new Exception("Unable to compare nodes with different number of children.");
            }

            for (int i = 0; i < reference.ChildNodes.Count; i++)
            {
                Compare((ActivityDO)reference.ChildNodes[i], (ActivityDO)actual.ChildNodes[i], callback);
            }
        }

        private void Visit(ActivityDO activity, Action<ActivityDO> callback)
        {
            callback(activity);

            foreach (var child in activity.ChildNodes.OfType<ActivityDO>().ToArray())
            {
                Visit(child, callback);
            }
        }

        //[Test,Ignore("plugin transmitter in v2 doesn't allow anything except ActioDTO as input param")]
        //public async void CanProcessDocuSignTemplate()
        //{
            // Test.
//            Action action = new Action();
//            var plan = FixtureData.TestRoute2();
//            var payloadMappings = FixtureData.FieldMappings;
//            var actionDo = FixtureData.IntegrationTestAction();
//            actionDo.ActivityTemplate.Plugin.Endpoint = "localhost:53234";
//            ProcessDO procesDO = FixtureData.TestProcess1();
//            PluginTransmitterMock
//                .Setup(m => m.CallActionAsync<ActionDataPackageDTO, ActionDTO>(
//                    It.Is<string>(s => s == "testaction"),
//                    It.IsAny<ActionDataPackageDTO>()))
//                .Returns(() => Task.FromResult(Mapper.Map<ActionDTO>(actionDo)))
//                .Verifiable();
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                uow.RouteRepository.Add(plan);
//                uow.ActionRepository.Add(actionDo);
//                uow.ActionListRepository.Add((ActionListDO)actionDo.ParentActivity);
//                uow.ProcessRepository.Add(((ActionListDO)actionDo.ParentActivity).Process);
//                uow.SaveChanges();
//
//                await action.PrepareToExecute(actionDo, procesDO, uow);
//            }
//
//            //Ensure that no Incidents were registered
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                Assert.IsFalse(uow.IncidentRepository.GetAll().Any(i => i.PrimaryCategory == "Envelope"));
//            }
//
//            //We use a mock of IPluginTransmitter. Get that mock and check that 
//            //CallActionAsync was called with the correct attributes
//            // TODO: Fix this line according to v2 changes
//            PluginTransmitterMock.Verify();
        //}

        // DO-1270
//        [Test]
//        public void Process_ActionNotUnstarted_ThrowException()
//        {
//            ActionDO actionDo = FixtureData.TestAction9();
//            Action _action = ObjectFactory.GetInstance<Action>();
//            ProcessDO procesDo = FixtureData.TestContainer1();
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                Assert.AreEqual("Action ID: 2 status is 4.", _action.PrepareToExecute(actionDo, procesDo, uow).Exception.InnerException.Message);
//            }
//        }

//        [Test, Ignore("Ignored execution related tests. Refactoring is going on")]
//        public void Process_ReturnJSONDispatchError_ActionStateError()
//        {
//            ActionDO activityDO = FixtureData.IntegrationTestAction();
//            ProcessDO procesDo = FixtureData.TestProcess1();
//            var pluginClientMock = new Mock<IPluginTransmitter>();
//            pluginClientMock.Setup(s => s.CallActionAsync<ActionDTO>(It.IsAny<string>(), It.IsAny<ActionDTO>())).ThrowsAsync(new RestfulServiceException());
//            ObjectFactory.Configure(cfg => cfg.For<IPluginTransmitter>().Use(pluginClientMock.Object));
//            //_action = ObjectFactory.GetInstance<IAction>();
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                _action.PrepareToExecute(activityDO, procesDo, uow);
//            }
//
//            Assert.AreEqual(ActionState.Error, activityDO.ActionState);
//        }

//        [Test]
//        public void Process_ReturnJSONDispatchNotError_ActionStateCompleted()
//        {
//            ActionDO activityDO = FixtureData.IntegrationTestAction();
//            activityDO.ActivityTemplate.Plugin.Endpoint = "http://localhost:53234/actions/configure";
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                uow.ActivityTemplateRepository.Add(activityDO.ActivityTemplate);
//                uow.ActionRepository.Add(activityDO);
//                uow.SaveChanges();
//            }
//
//            ProcessDO procesDO = FixtureData.TestProcess1();
//            var pluginClientMock = new Mock<IPluginTransmitter>();
//            pluginClientMock.Setup(s => s.CallActionAsync<ActionDTO>(It.IsAny<string>(), It.IsAny<ActionDTO>())).Returns<string, ActionDTO>((s, a) => Task.FromResult(a));
//            ObjectFactory.Configure(cfg => cfg.For<IPluginTransmitter>().Use(pluginClientMock.Object));
//            //_action = ObjectFactory.GetInstance<IAction>();
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//
//                _action.PrepareToExecute(activityDO, procesDO, uow);
//            }
//
//            Assert.AreEqual(ActionState.Active, activityDO.ActionState);
//        }

        [Test]
        public void Process_ActionUnstarted_ShouldBeCompleted()
        {
            //Arrange
            ActivityDO activityDo = FixtureData.TestActivityUnstarted();
            activityDo.ActivityTemplate.Terminal.Endpoint = "http://localhost:53234/actions/configure";
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO(){
                    Name="name",
                    RouteState = RouteState.Active,
                    ChildNodes = {activityDo}});
                uow.SaveChanges();
            }

            ActivityDTO activityDto = Mapper.Map<ActivityDTO>(activityDo);
            TerminalTransmitterMock.Setup(rc => rc.PostAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(() => Task.FromResult<string>(JsonConvert.SerializeObject(activityDto)));

            ContainerDO containerDO = FixtureData.TestContainer1();

            //Act
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var response = _activity.PrepareToExecute(activityDo, ActionState.InitialRun, containerDO, uow);

                //Assert
                Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            }
        }

        [Test]
        public void AddCrate_AddCratesDTO_UpdatesActionCratesStorage()
        {
            ActivityDO activityDO = FixtureData.TestActivity23();

            using (var updater = _crate.UpdateStorage(activityDO))
            {
                updater.CrateStorage.AddRange(FixtureData.CrateStorageDTO());
            }

            Assert.IsNotEmpty(activityDO.CrateStorage);
        }

// DO-1270
//        [Test]
//        [ExpectedException(ExpectedMessage = "Action ID: 2 status is 4.")]
//        public async void ActionStateActive_ThrowsException()
//        {
//            ActionDO actionDo = FixtureData.TestActionStateActive();
//            Action _action = ObjectFactory.GetInstance<Action>();
//            ProcessDO procesDo = FixtureData.TestProcess1();
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                await _action.PrepareToExecute(actionDo, procesDo, uow);
//            }
//        }
//
//        [Test]
//        [ExpectedException(ExpectedMessage = "Action ID: 2 status is 4.")]
//        public async void ActionStateDeactive_ThrowsException()
//        {
//            ActionDO actionDo = FixtureData.TestActionStateDeactive();
//            Action _action = ObjectFactory.GetInstance<Action>();
//            ProcessDO procesDo = FixtureData.TestProcess1();
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                await _action.PrepareToExecute(actionDo, procesDo, uow);
//            }
//        }

//        [Test]
//        [ExpectedException(ExpectedMessage = "Action ID: 2 status is 4.")]
//        public async void ActionStateError_ThrowsException()
//        {
//            ActionDO actionDo = FixtureData.TestActionStateError();
//            Action _action = ObjectFactory.GetInstance<Action>();
//            ProcessDO procesDo = FixtureData.TestProcess1();
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                await _action.PrepareToExecute(actionDo, procesDo, uow);
//            }
//        }

        [Test]
        public async void PrepareToExecute_WithMockedExecute_WithoutPayload()
        {
            ActivityDO activityDo = FixtureData.TestActivityStateInProcess();
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name="sdfsdf",
                    RouteState = RouteState.Active,
                    ChildNodes = { activityDo }
                });
                uow.SaveChanges();
            }

            Activity _activity = ObjectFactory.GetInstance<Action>();
            ContainerDO containerDO = FixtureData.TestContainer1();
            EventManager.EventActionStarted += EventManager_EventActionStarted;
            var executeActionMock = new Mock<IActivity>();
            executeActionMock.Setup(s => s.Run(It.IsAny<IUnitOfWork>(), activityDo, It.IsAny<ActionState>(), containerDO)).Returns<Task<PayloadDTO>>(null);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var count = uow.PlanRepository.GetActivityQueryUncached().Count();
                await _activity.PrepareToExecute(activityDo, ActionState.InitialRun, containerDO, uow);
                //Assert.AreEqual(uow.ActionRepository.GetAll().Count(), count + 1);
            }
            Assert.IsNull(containerDO.CrateStorage);
            Assert.IsTrue(_eventReceived);
           // Assert.AreEqual(actionDo.ActionState, ActionState.Active);
        }

        [Test]
        public async void PrepareToExecute_WithMockedExecute_WithPayload()
        {
            ActivityDO activityDo = FixtureData.TestActivityStateInProcess();
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO() { Label = "Test Action" });

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    RouteState = RouteState.Active,
                    ChildNodes = { activityDo }
                });
                uow.SaveChanges();
            }

            IActivity _activity = ObjectFactory.GetInstance<IActivity>();
            ContainerDO containerDO = FixtureData.TestContainer1();
            EventManager.EventActionStarted += EventManager_EventActionStarted;

            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalClientMock = new Mock<ITerminalTransmitter>();
                terminalClientMock.Setup(s => s.CallActionAsync<PayloadDTO>(It.IsAny<string>(), It.IsAny<Fr8DataDTO>(), It.IsAny<string>()))
                                .Returns(Task.FromResult(new PayloadDTO(containerDO.Id)
                                {
                                    CrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(activityDo.CrateStorage)
                                }));
                ObjectFactory.Configure(cfg => cfg.For<ITerminalTransmitter>().Use(terminalClientMock.Object));

                var count = uow.PlanRepository.GetActivityQueryUncached().Count();
                await _activity.PrepareToExecute(activityDo, ActionState.InitialRun, containerDO, uow);
                //Assert.AreEqual(uow.ActionRepository.GetAll().Count(), count + 1);
            }
            Assert.IsNotNull(containerDO.CrateStorage);
            Assert.IsTrue(_eventReceived);
           // Assert.AreEqual(actionDo.ActionState, ActionState.Active);
        }

        [Test]
        public async void ActionStarted_EventRaisedSuccessfully()
        {
            ActivityDO activityDo = FixtureData.TestActivityStateInProcess();
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name="name",
                    RouteState = RouteState.Active,
                    ChildNodes = { activityDo }
                });
                uow.SaveChanges();
            }


            Activity _activity = ObjectFactory.GetInstance<Activity>();
            ContainerDO containerDO = FixtureData.TestContainer1();
            EventManager.EventActionStarted += EventManager_EventActionStarted;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var count = uow.PlanRepository.GetActivityQueryUncached().Count();
                await _activity.PrepareToExecute(activityDo, ActionState.InitialRun, containerDO, uow);
                //Assert.AreEqual(uow.ActionRepository.GetAll().Count(), count + 1);
            }
            Assert.IsTrue(_eventReceived);
        //            Assert.AreEqual(actionDo.ActionState, ActionState.Active);
        }

        private void EventManager_EventActionStarted(ActivityDO activity)
        {
            _eventReceived = true;
        }

        // DO-1214
//        private void UpdateDatabase(ActionDO curActionDo)
//        {
//
//            curActionDo.ActivityTemplate.Plugin.Endpoint = "pluginDocusign";
//            _uow.ActivityTemplateRepository.Add(curActionDo.ActivityTemplate);
//            _uow.SaveChanges();
//
//            _uow.RouteRepository.Add(FixtureData.TestRoute1());
//
//            ActionListDO parentActivity = (ActionListDO)curActionDo.ParentActivity;
//            parentActivity.Process.RouteId = 33;
//            _uow.ProcessRepository.Add(parentActivity.Process);
//            _uow.SaveChanges();
//
//            _uow.ActionListRepository.Add(parentActivity);
//            _uow.SaveChanges();
//
//            _uow.ActionRepository.Add(curActionDo);
//            _uow.SaveChanges();
//        }
    }

    internal class TestActionService : Action
    {
        private ITerminalTransmitter _restfulServiceClient;

        internal ITerminalTransmitter RestfulServiceClient
        {
            get
            {
                if (_restfulServiceClient == null)
                {
                    _restfulServiceClient = new Mock<ITerminalTransmitter>(MockBehavior.Default).Object;
                }

                return _restfulServiceClient;
            }
            private set { _restfulServiceClient = value; }
        }
    }
}
