using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StructureMap;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalInterface = Hub.Interfaces;
using Data.Interfaces;
using Hub.Managers;
using UtilitiesTesting;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Data.States;
using DockyardTest.Services.Container;
using Hub.Exceptions;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ContainerExecute")]
    public class ContainerExecutionTests : BaseTest
    {

        private ActivityServiceMock ActivityService;
        private InternalInterface.IContainer _container;
        private ICrateManager _crateManager;
        private InternalInterface.IPlan _plan;

        [SetUp]
        //constructor method as it is run at the test start
        public override void SetUp()
        {
            base.SetUp();

            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            ActivityService = new ActivityServiceMock(ObjectFactory.GetInstance<InternalInterface.IActivity>());
            ObjectFactory.Container.Inject(typeof (InternalInterface.IActivity), ActivityService);
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
            _plan = ObjectFactory.GetInstance<InternalInterface.IPlan>();

            FixtureData.AddTestActivityTemplate();
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task Execute_ContainerDoIsNull_ThrowsArgumentNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _container.Run(uow, null);
            }
        }

        [Test]
        public async Task CanExecuteLinearSequence()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO) plan.ChildNodes[0];

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                }, ActivityService.ExecutedActivities);
            }
        }


        [Test]
        public async Task ExecuteOnlyStartingSubplanByDefault()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                            }
                        },
                        new SubPlanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(3),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 1
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        public async Task CanExecuteSequenceWithChildren()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(4),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(5),
                                            Ordering = 3
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(6),
                                            Ordering = 4,
                                            ChildNodes =
                                            {
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = 1,
                                                    Id = FixtureData.GetTestGuidById(7),
                                                    Ordering = 1
                                                },
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = 1,
                                                    Id = FixtureData.GetTestGuidById(8),
                                                    Ordering = 2
                                                },
                                            }
                                        }
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(9),
                                    Ordering = 3,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(10),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(11),
                                            Ordering = 2
                                        },
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(12),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(6)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(7)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(8)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(6)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(9)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(10)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(11)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(9)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(12)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        public async Task CanRecoverSequenceWithChildren()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(4),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(5),
                                            Ordering = 3
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(6),
                                            Ordering = 4,
                                            ChildNodes =
                                            {
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = 1,
                                                    Id = FixtureData.GetTestGuidById(7),
                                                    Ordering = 1
                                                },
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = 1,
                                                    Id = FixtureData.GetTestGuidById(8),
                                                    Ordering = 2
                                                },
                                            }
                                        }
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(9),
                                    Ordering = 3,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(10),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(11),
                                            Ordering = 2
                                        },
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(12),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO) plan.ChildNodes[0];
                ActivityService.CustomActivities[FixtureData.GetTestGuidById(4)] = new SuspenderActivityMock(_crateManager);

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                var container = uow.ContainerRepository.GetQuery().Single(x => x.PlanId == plan.Id);

                using (var storage = _crateManager.UpdateStorage(() => container.CrateStorage))
                {
                    var opState = storage.CrateContentsOfType<OperationalStateCM>().First();

                    container.CurrentActivityId = opState.CallStack.Peek().NodeId;
                    opState.CallStack = null;
                }

                uow.SaveChanges();

                ActivityService.CustomActivities.Remove(FixtureData.GetTestGuidById(4)); // we are not interested in suspender logic testing here. Just allow activity to run by default.
                await _plan.Continue(container.Id);
                
                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)), // second call is because of we've resumed execution
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(6)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(7)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(8)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(6)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(9)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(10)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(11)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(9)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(12)),
                }, ActivityService.ExecutedActivities);
            }
        }
        
        [Test]
        public async Task CanJump()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new JumperActivityMock(_crateManager, FixtureData.GetTestGuidById(5));

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        [ExpectedException(typeof(ActivityExecutionException))]
        public async Task FailToJumpBetwenLevels()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(3),
                                            Ordering = 2
                                        },
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 4,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(6),
                                            Ordering = 2
                                        },
                                    }
                                }
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new JumperActivityMock(_crateManager, FixtureData.GetTestGuidById(5));

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        public async Task CanJumpToSelf()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(2)] = new JumpToSelfActivityMock(_crateManager);

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        public async Task CanJumpToSubplan()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                 new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                            }
                        },
                        new SubPlanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(4),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(6),
                                    Ordering = 2
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(2)] = new SubplanJumperActivityMock(_crateManager, FixtureData.GetTestGuidById(4));

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(6)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        public async Task CanCallSubplan()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                 new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                            }
                        },
                        new SubPlanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(4),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(6),
                                    Ordering = 2
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(2)] = new CallerActivityMock(_crateManager, FixtureData.GetTestGuidById(4));

                await _plan.Run(uow, plan);
                
                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(5)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(6)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);
            }
        }


        [Test]
        public async Task CanSuspendAndContinueExecution()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                 new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                            }
                        },
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new SuspenderActivityMock(_crateManager);

                await _plan.Run(uow, plan);

                Assert.AreEqual(State.Suspended, uow.ContainerRepository.GetQuery().Single(x => x.PlanId == plan.Id).State, "Invalid container state");
                AssertExecutionSequence(new[]
               {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);

                await _plan.Continue(uow.ContainerRepository.GetQuery().Single(x => x.PlanId == plan.Id).Id);

                Assert.AreEqual(1, uow.ContainerRepository.GetQuery().Count(x => x.PlanId == plan.Id), "New container was stared");

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                }, ActivityService.ExecutedActivities);
            }
        }

        [Test]
        public async Task CanLoopAndCall()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(4),
                                            Ordering = 1
                                        }
                                    }
                                },

                            }
                        },
                        new SubPlanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(5),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = 1,
                                    Id = FixtureData.GetTestGuidById(6),
                                    Ordering = 1,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = 1,
                                            Id = FixtureData.GetTestGuidById(7),
                                            Ordering = 1
                                        },
                                    }
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new LooperActivityMock(_crateManager, 2);
                ActivityService.CustomActivities[FixtureData.GetTestGuidById(4)] = new CallerActivityMock(_crateManager, FixtureData.GetTestGuidById(5));

                await _plan.Run(uow, plan);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                        new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                        new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(6)),
                            new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(7)),
                        new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(6)),
                    new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                        new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                        new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(6)),
                            new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(7)),
                        new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(6)),
                   new ActivityExecutionCall(ActivityExecutionMode.ReturnFromChildren, FixtureData.GetTestGuidById(3)),
                   new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);
            }
        }


        private void AssertExecutionSequence(ActivityExecutionCall[] expected, List<ActivityExecutionCall> actual)
        {
            Assert.AreEqual(expected.Length, actual.Count, "Invalid count of activity executions");

            for (int i = 0; i < expected.Length; i ++)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id, $"Invalid activtiy is executed at step {i}");
                Assert.AreEqual(expected[i].Mode, actual[i].Mode, $"Invalid activtiy execution mode at step {i}");
            }
        }
    }
}
