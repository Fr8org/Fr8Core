using System;
using System.Linq;
using NUnit.Framework;
using StructureMap;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using Data.Interfaces;
using System.Threading.Tasks;
using Data.Entities;
using Data.States;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using HubTests.Services.Container;
using Hub.Exceptions;
using Fr8.Testing.Unit.Fixtures;
using Fr8.Infrastructure.Data.States;

namespace HubTests.Services
{
    [TestFixture]
    [Category("ContainerExecute")]
    public class ContainerExecutionTests : ContainerExecutionTestBase
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Execute_ContainerDoIsNull_ThrowsArgumentNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await ContainerService.Run(uow, null);
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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId =FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                            }
                        },
                        new SubplanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(3),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 1
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(4),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(5),
                                            Ordering = 3
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(6),
                                            Ordering = 4,
                                            ChildNodes =
                                            {
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                                    Id = FixtureData.GetTestGuidById(7),
                                                    Ordering = 1
                                                },
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                                    Id = FixtureData.GetTestGuidById(8),
                                                    Ordering = 2
                                                },
                                            }
                                        }
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(9),
                                    Ordering = 3,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(10),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(11),
                                            Ordering = 2
                                        },
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(12),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(4),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(5),
                                            Ordering = 3
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(6),
                                            Ordering = 4,
                                            ChildNodes =
                                            {
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                                    Id = FixtureData.GetTestGuidById(7),
                                                    Ordering = 1
                                                },
                                                new ActivityDO()
                                                {
                                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                                    Id = FixtureData.GetTestGuidById(8),
                                                    Ordering = 2
                                                },
                                            }
                                        }
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(9),
                                    Ordering = 3,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(10),
                                            Ordering = 1
                                        },
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(11),
                                            Ordering = 2
                                        },
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(12),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(4)] = new SuspenderActivityMock(CrateManager);

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

                var container = uow.ContainerRepository.GetQuery().Single(x => x.PlanId == plan.Id);

                using (var storage = CrateManager.UpdateStorage(() => container.CrateStorage))
                {
                    var opState = storage.CrateContentsOfType<OperationalStateCM>().First();

                    container.CurrentActivityId = opState.CallStack.TopFrame.NodeId;
                    opState.CallStack = null;
                }

                uow.SaveChanges();

                ActivityService.CustomActivities.Remove(FixtureData.GetTestGuidById(4)); // we are not interested in suspender logic testing here. Just allow activity to run by default.
                await ContainerService.Continue(uow, container);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new JumperActivityMock(CrateManager, FixtureData.GetTestGuidById(5));

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(3),
                                            Ordering = 2
                                        },
                                    }
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 4,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(6),
                                            Ordering = 2
                                        },
                                    }
                                }
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new JumperActivityMock(CrateManager, FixtureData.GetTestGuidById(5));

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(2)] = new JumpToSelfActivityMock(CrateManager);

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                 new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                            }
                        },
                        new SubplanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(4),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(6),
                                    Ordering = 2
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(2)] = new SubplanJumperActivityMock(CrateManager, FixtureData.GetTestGuidById(4));

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId =FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                 new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                            }
                        },
                        new SubplanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(4),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(5),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(6),
                                    Ordering = 2
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;
                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(2)] = new CallerActivityMock(CrateManager, FixtureData.GetTestGuidById(4));

                await Plan.Run(plan.Id, null, null);

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                 new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                            }
                        },
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new SuspenderActivityMock(CrateManager);

                await Plan.Run(plan.Id, null, null);

                Assert.AreEqual(State.Suspended, uow.ContainerRepository.GetQuery().Single(x => x.PlanId == plan.Id).State, "Invalid container state");
                AssertExecutionSequence(new[]
               {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);

                await ContainerService.Continue(uow, uow.ContainerRepository.GetQuery().Single(x => x.PlanId == plan.Id));

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
                        new SubplanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },

                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(4),
                                            Ordering = 1
                                        }
                                    }
                                },

                            }
                        },
                        new SubplanDO(false)
                        {
                            Id = FixtureData.GetTestGuidById(5),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(6),
                                    Ordering = 1,
                                    ChildNodes =
                                    {
                                        new ActivityDO()
                                        {
                                            ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                            Id = FixtureData.GetTestGuidById(7),
                                            Ordering = 1
                                        },
                                    }
                                },
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];

                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new LooperActivityMock(CrateManager, 2);
                ActivityService.CustomActivities[FixtureData.GetTestGuidById(4)] = new CallerActivityMock(CrateManager, FixtureData.GetTestGuidById(5));

                await Plan.Run(plan.Id, null, null);

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
    }
}
