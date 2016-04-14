using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Terminal;
using Hub.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Action = Hub.Services.Activity;


namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActivityService")]
    public class ActivityServiceTests2 : BaseTest
    {
        private IActivity _activity;
        private ICrateManager _crate;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActivityTemplateDO> _pr1Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActivityTemplateDO> _pr2Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "SQL Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "SQL Read", Version = "1.0" } };
        private bool _eventReceived;
        private BaseTerminalActivity _baseTerminalActivity;
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
            _baseTerminalActivity = new BaseTerminalActivity();
            _terminal = ObjectFactory.GetInstance<Terminal>();

            FixtureData.AddTestActivityTemplate();
        }

        // DO-1214
        //        [Test]
        //        public async Task Action_Configure_ExistingActionShouldBeUpdatedWithNewAction()
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
        public async Task Activity_Configure_WithNullActionTemplate_ThrowsArgumentNullException()
        {
            var _service = new Activity(ObjectFactory.GetInstance<ICrateManager>(), ObjectFactory.GetInstance<IAuthorization>(), ObjectFactory.GetInstance<ISecurityServices>(), ObjectFactory.GetInstance<IActivityTemplate>(), ObjectFactory.GetInstance<IPlanNode>());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _service.Configure(uow, null, null);
            }
        }

        [Test]
        public async Task CanCRUDActivities()
        {
            ActivityDO origActivityDO;

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan1();
                uow.PlanRepository.Add(plan);

                var subPlane = FixtureData.TestSubPlanDO1();
                plan.ChildNodes.Add(subPlane);

                origActivityDO = new FixtureData(uow).TestActivity3();

                origActivityDO.ParentPlanNodeId = subPlane.Id;

                uow.ActivityTemplateRepository.Add(origActivityDO.ActivityTemplate);
                uow.SaveChanges();
            }

            IActivity activity = new Activity(ObjectFactory.GetInstance<ICrateManager>(), ObjectFactory.GetInstance<IAuthorization>(), ObjectFactory.GetInstance<ISecurityServices>(), ObjectFactory.GetInstance<IActivityTemplate>(), ObjectFactory.GetInstance<IPlanNode>());

            //Add
                await activity.SaveOrUpdateActivity(origActivityDO);

            ActivityDO activityDO;
            //Get
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activityDO = activity.GetById(uow, origActivityDO.Id);
            }

            Assert.AreEqual(origActivityDO.Id, activityDO.Id);
            Assert.AreEqual(origActivityDO.CrateStorage, activityDO.CrateStorage);

            Assert.AreEqual(origActivityDO.Ordering, activityDO.Ordering);

            ISubPlan subPlan = new SubPlan();
            //Delete
            await subPlan.DeleteActivity(null, activityDO.Id, true);
        }

        [Test]
        public async Task ActivityWithNestedUpdated_StructureUnchanged()
        {
            var tree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();
            var updatedTree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO
                {
                    PlanState = PlanState.Active,
                    Name = "name",
                    ChildNodes = {tree}
                };
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();

                Visit(updatedTree, x => x.Label = string.Format("We were here {0}", x.Id));
            }

            await _activity.SaveOrUpdateActivity(updatedTree);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            { 
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
        public async Task ActivityWithNestedUpdated_RemoveElements()
        {
            var tree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();
            var updatedTree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO
                {
                    PlanState = PlanState.Active,
                    Name = "name",
                    ChildNodes = {tree}
                };
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
                int removeCounter = 0;

                Visit(updatedTree, a =>
                {
                    if (removeCounter%3 == 0 && a.ParentPlanNode != null)
                    {
                        a.ParentPlanNode.ChildNodes.Remove(a);
                    }

                    removeCounter++;
                });
            }
                
            await _activity.SaveOrUpdateActivity(updatedTree);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
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
        public async Task ActivityWithNestedUpdated_AddElements()
        {
            var tree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();
            var updatedTree = FixtureData.CreateTestActivityTreeWithOnlyActivityDo();


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO
                {
                    Name = "name",
                    PlanState = PlanState.Active,
                    ChildNodes = {tree}
                };

                uow.PlanRepository.Add(plan);
                uow.SaveChanges();

                int addCounter = 0;

                Visit(updatedTree, a =>
                {
                    if (addCounter%3 == 0 && a.ParentPlanNode != null)
                    {
                        var newAction = new ActivityDO
                        {
                            Id = FixtureData.GetTestGuidById(addCounter + 666),
                            ParentPlanNode = a,
                            ActivityTemplateId = Guid.NewGuid()
                        };

                        a.ParentPlanNode.ChildNodes.Add(newAction);
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
                                ParentPlanNode = a,
                                ActivityTemplateId = Guid.NewGuid()
                            };

                            a.ParentPlanNode.ChildNodes.Add(newAction);
                        }

                        addCounter++;
                    });
                }

                updatedTree.ParentPlanNodeId = plan.Id;
            }

            await _activity.SaveOrUpdateActivity( updatedTree);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
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
        //public async Task CanProcessDocuSignTemplate()
        //{
        // Test.
        //            Action action = new Action();
        //            var plan = FixtureData.TestPlan2();
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
        //                uow.PlanRepository.Add(plan);
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
        //            activityDO.ActivityTemplate.Plugin.Endpoint = "http://localhost:53234/activities/configure";
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
        public void Process_ActivityUnstarted_ShouldBeCompleted()
        {
            //Arrange
            ActivityDO activityDo = FixtureData.TestActivityUnstarted();
            activityDo.ActivityTemplate.Terminal.Endpoint = "http://localhost:53234/activities/configure";
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO(){
                    Name="name",
                    PlanState = PlanState.Active,
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
                var response = _activity.PrepareToExecute(activityDo, ActivityState.InitialRun, containerDO, uow);

                //Assert
                Assert.That(response.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            }
        }

        [Test]
        public void AddCrate_AddCratesDTO_UpdatesActivityCratesStorage()
        {
            ActivityDO activityDO = FixtureData.TestActivity23();

            using (var crateStorage = _crate.GetUpdatableStorage(activityDO))
            {
                crateStorage.AddRange(FixtureData.CrateStorageDTO());
            }

            Assert.IsNotEmpty(activityDO.CrateStorage);
        }

        // DO-1270
        //        [Test]
        //        [ExpectedException(ExpectedMessage = "Action ID: 2 status is 4.")]
        //        public async Task ActionStateActive_ThrowsException()
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
        //        public async Task ActionStateDeactive_ThrowsException()
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
        //        public async Task ActionStateError_ThrowsException()
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
        public async Task PrepareToExecute_WithMockedExecute_WithoutPayload()
        {
            ActivityDO activityDo = FixtureData.TestActivityStateInProcess();
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name="sdfsdf",
                    PlanState = PlanState.Active,
                    ChildNodes = { activityDo }
                });
                uow.SaveChanges();
            }

            Activity _activity = ObjectFactory.GetInstance<Action>();
            ContainerDO containerDO = FixtureData.TestContainer1();
            EventManager.EventActionStarted += EventManager_EventActivityStarted;
            var executeActionMock = new Mock<IActivity>();
            executeActionMock.Setup(s => s.Run(It.IsAny<IUnitOfWork>(), activityDo, It.IsAny<ActivityState>(), containerDO)).Returns<Task<PayloadDTO>>(null);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var count = uow.PlanRepository.GetActivityQueryUncached().Count();
                await _activity.PrepareToExecute(activityDo, ActivityState.InitialRun, containerDO, uow);
                //Assert.AreEqual(uow.ActionRepository.GetAll().Count(), count + 1);
            }
            Assert.IsNull(containerDO.CrateStorage);
            Assert.IsTrue(_eventReceived);
           // Assert.AreEqual(actionDo.ActionState, ActionState.Active);
        }

        [Test]
        public async Task PrepareToExecute_WithMockedExecute_WithPayload()
        {
            ActivityDO activityDo = FixtureData.TestActivityStateInProcess();
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO() { Label = "Test Action" });

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    PlanState = PlanState.Active,
                    ChildNodes = { activityDo }
                });
                uow.SaveChanges();
            }

            IActivity _activity = ObjectFactory.GetInstance<IActivity>();
            ContainerDO containerDO = FixtureData.TestContainer1();
            EventManager.EventActionStarted += EventManager_EventActivityStarted;

            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalClientMock = new Mock<ITerminalTransmitter>();
                terminalClientMock.Setup(s => s.CallActivityAsync<PayloadDTO>(It.IsAny<string>(), It.IsAny<Fr8DataDTO>(), It.IsAny<string>()))
                                .Returns(Task.FromResult(new PayloadDTO(containerDO.Id)
                                {
                                    CrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(activityDo.CrateStorage)
                                }));
                ObjectFactory.Configure(cfg => cfg.For<ITerminalTransmitter>().Use(terminalClientMock.Object));

                var count = uow.PlanRepository.GetActivityQueryUncached().Count();
                await _activity.PrepareToExecute(activityDo, ActivityState.InitialRun, containerDO, uow);
                //Assert.AreEqual(uow.ActionRepository.GetAll().Count(), count + 1);
            }
            Assert.IsNotNull(containerDO.CrateStorage);
            Assert.IsTrue(_eventReceived);
           // Assert.AreEqual(actionDo.ActionState, ActionState.Active);
        }

        [Test]
        public async Task ActivityStarted_EventRaisedSuccessfully()
        {
            ActivityDO activityDo = FixtureData.TestActivityStateInProcess();
            activityDo.CrateStorage = JsonConvert.SerializeObject(new ActivityDTO());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(activityDo.ActivityTemplate);
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name="name",
                    PlanState = PlanState.Active,
                    ChildNodes = { activityDo }
                });
                uow.SaveChanges();
            }


            Activity _activity = ObjectFactory.GetInstance<Activity>();
            ContainerDO containerDO = FixtureData.TestContainer1();
            EventManager.EventActionStarted += EventManager_EventActivityStarted;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var count = uow.PlanRepository.GetActivityQueryUncached().Count();
                await _activity.PrepareToExecute(activityDo, ActivityState.InitialRun, containerDO, uow);
                //Assert.AreEqual(uow.ActionRepository.GetAll().Count(), count + 1);
            }
            Assert.IsTrue(_eventReceived);
        //            Assert.AreEqual(actionDo.ActionState, ActionState.Active);
        }

        private void EventManager_EventActivityStarted(ActivityDO activity)
        {
            _eventReceived = true;
        }

        [Test]
        public void ActivityController_GetSolutionList()
        {
            //Arrange
            //Add two activity templates to the database
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(FixtureData.TestActivityTemplateDO3());
                uow.ActivityTemplateRepository.Add(FixtureData.TestActivityTemplateDO4());
                uow.SaveChanges();
            }
            //Act
            //Call the activities/GetTerminalSolutionList?terminalName=terminalDocuSign
            var solutionList = _activity.GetSolutionNameList("terminalDocuSign");
            //Assert
            Assert.True(solutionList.Any());
            Assert.True(solutionList.Count == 2);
            Assert.Contains("Mail_Merge_Into_DocuSign", solutionList);
            Assert.Contains("Extract_Data_From_Envelopes", solutionList);
        }

        // DO-1214
//        private void UpdateDatabase(ActionDO curActionDo)
//        {
//
//            curActionDo.ActivityTemplate.Plugin.Endpoint = "pluginDocusign";
//            _uow.ActivityTemplateRepository.Add(curActionDo.ActivityTemplate);
//            _uow.SaveChanges();
//
//            _uow.PlanRepository.Add(FixtureData.TestPlan1());
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

}
