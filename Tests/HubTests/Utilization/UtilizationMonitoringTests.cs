using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Utilization;
using Data.States;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using HubTests.Services.Container;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Utilization
{
    [TestFixture]
    [Category("UtilizationMonitoring")]
    public class UtilizationMonitoringTests : ContainerExecutionTestBase
    {
        private readonly ManualyTriggeredTimerService _timerService = new ManualyTriggeredTimerService();

        public class UtilizationDataProviderMock : MockedUtilizationDataProvider
        {
            private readonly Dictionary<string, ActivityExecutionRate> _rates = new Dictionary<string, ActivityExecutionRate>();
            
            public ActivityExecutionRate[] GetRates()
            {
                lock (_rates)
                {
                    return _rates.Values.ToArray();
                }
            }

            public override void UpdateActivityExecutionRates(ActivityExecutionRate[] reports)
            {
                lock (_rates)
                {
                    foreach (var activityExecutionRate in reports)
                    {
                        _rates[activityExecutionRate.UserId] = activityExecutionRate;
                    }
                }
            }

            public void AssertRates(string userId, int expected)
            {
                lock (_rates)
                {
                    ActivityExecutionRate rates;

                    if (!_rates.TryGetValue(userId, out rates))
                    {
                        Assert.Fail($"No activities were tracked for user \"{userId}\"");
                    }
                    Assert.AreEqual(expected, rates.ActivitiesExecuted, $"Invalid number of activities were tracked for user \"{userId}\"");
                }
            }
        }

        private UtilizationDataProviderMock _provider;

        public override void TearDown()
        {
            _timerService.Clear();
            base.TearDown();
        }

        protected override void InitializeContainer()
        {
            _provider = new UtilizationDataProviderMock();

            CloudConfigurationManager.RegisterApplicationSettings(new ConfigurationOverride(CloudConfigurationManager.AppSettings).Set("UtilizationReportAggregationUnit", "1"));
            ObjectFactory.Container.Inject(typeof(IUtilizationDataProvider), _provider);
            ObjectFactory.Container.Inject<ITimer>(_timerService);
        }

        [Test]
        public void CanMonitorActivityExecution()
        {
            var monitoringService = ObjectFactory.Container.GetInstance<IUtilizationMonitoringService>();

            var activity = new ActivityDO
            {
                Fr8AccountId = "1"
            };

            var fr8Container = new ContainerDO();

            for (int i = 0; i < 100; i++)
            {
                monitoringService.TrackActivityExecution(activity, fr8Container);
            }

            activity.Fr8AccountId = "2";

            for (int i = 0; i < 57; i++)
            {
                monitoringService.TrackActivityExecution(activity, fr8Container);
            }

            activity.Fr8AccountId = "3";

            for (int i = 0; i < 202; i++)
            {
                monitoringService.TrackActivityExecution(activity, fr8Container);
            }

            _timerService.Tick();
            _timerService.Tick();

            _provider.AssertRates("1", 100);
            _provider.AssertRates("2", 57);
            _provider.AssertRates("3", 202);
        }

        [Test]
        public async Task IsCalledDuringContainerExecution()
        {
            Fr8AccountDO userAcct;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                uow.SaveChanges();
            }

            PlanDO plan;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
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
                                    ActivityTemplateId =FixtureData.GetTestGuidById(1),
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
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];
                plan.Fr8Account = userAcct;

                uow.SaveChanges();

                await Plan.Run(plan.Id, null, null);

                _timerService.Tick();
                _timerService.Tick();

                _provider.AssertRates(userAcct.Id, 4);
            }
        }
    }
}
