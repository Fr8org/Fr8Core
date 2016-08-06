using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Constants;
using Hub.Interfaces;
using HubTests.Services.Container;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Utilization
{
    [TestFixture]
    [Category("UtilizationMonitoring")]
    public class ActivityExecutionRateLimiterIntTests : ContainerExecutionTestBase
    {
        private readonly ManualyTriggeredTimerService _timerService = new ManualyTriggeredTimerService();

        private class RateLimiterMock : IActivityExecutionRateLimitingService, IUtilizationMonitoringService
        {
            private int _activitiesExecuted;

            public int AggregationUnitDuration => 1;

            public bool CheckActivityExecutionRate(string fr8AccountId)
            {
                return _activitiesExecuted <= 1;
            }
            
            public void TrackActivityExecution(ActivityDO activity, ContainerDO container)
            {
                _activitiesExecuted ++;
            }
        }

        protected override void InitializeContainer()
        {
            var service = new RateLimiterMock();

            ObjectFactory.Container.Inject(typeof(IActivityExecutionRateLimitingService), service);
            ObjectFactory.Container.Inject(typeof(IUtilizationMonitoringService), service);
            ObjectFactory.Container.Inject<ITimer>(_timerService);
        }

        public override void TearDown()
        {
            _timerService.Clear();
            base.TearDown();
        }

        [Test]
        public async Task CanLimitExecution()
        {
            Fr8AccountDO userAcct;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                uow.SaveChanges();
            }

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
                                    Fr8AccountId = userAcct.Id,
                                    Ordering = 1

                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Fr8AccountId = userAcct.Id,
                                    Ordering = 2
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Fr8AccountId = userAcct.Id,
                                    Ordering = 3
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(5),
                                    Fr8AccountId = userAcct.Id,
                                    Ordering = 4
                                }
                            }
                        }
                    }
                });

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO) plan.ChildNodes[0];
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);
            }
        }
    }
}
