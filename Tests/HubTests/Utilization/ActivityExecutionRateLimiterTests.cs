using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Interfaces;
using Hub.Services;
using NUnit.Framework;
using StructureMap;
using Utilities.Configuration.Azure;
using UtilitiesTesting;

namespace HubTests.Utilization
{
    [TestFixture]
    [Category("UtilizationMonitoring")]
    public class ActivityExecutionRateLimiterTests : BaseTest
    {
        public class UtilizationDataProviderMock : MockedUtilizationDataProvider
        {
            private OverheatingUsersUpdateResults _readyResults;
            private string[] _overheatingUsers = new string[0];

            public int GetOverheatingUsersCount;

            public override void UpdateActivityExecutionRates(ActivityExecutionRate[] reports)
            {
            }

            public void SetOverheatingResults(OverheatingUsersUpdateResults results)
            {
                _readyResults = results;
            }

            public void SetOverheatingUsers(params string[] overheatingUsers)
            {
                _overheatingUsers = overheatingUsers;
            }

            public override string[] GetOverheatingUsers()
            {
                GetOverheatingUsersCount++;

                return _overheatingUsers;
            }

            public override OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold)
            {
                if (_readyResults != null)
                {
                    return _readyResults;
                }

                return base.UpdateOverheatingUsers(threshold);
            }
        }

        private UtilizationDataProviderMock _provider;

        public override void SetUp()
        {
            base.SetUp();
            _provider = new UtilizationDataProviderMock();

            CloudConfigurationManager.RegisterApplicationSettings(new ConfigurationOverride(CloudConfigurationManager.AppSettings)
                                                                  .Set("UtilizationReportAggregationUnit", "1")
                                                                  .Set("UtilizationSateRenewInterval", "1"));

            ObjectFactory.Container.Inject(typeof(IUtilizationDataProvider), _provider);
        }

        [Test]
        public void CanInitialize()
        {
            CloudConfigurationManager.RegisterApplicationSettings(new ConfigurationOverride(CloudConfigurationManager.AppSettings)
                                                                  .Set("UtilizationReportAggregationUnit", "1")
                                                                  .Set("UtilizationSateRenewInterval", "1000"));

            _provider.SetOverheatingUsers("1", "2", "3");

            var rateLimiter = ObjectFactory.GetInstance<IActivityExecutionRateLimitingService>();

            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("1"), "User \"1\" should be banned");
            Assert.IsTrue(rateLimiter.CheckActivityExecutionRate("4"), "User \"4\" should not be banned");

            Assert.AreEqual(1, _provider.GetOverheatingUsersCount, "Should load overheating users from the provider only once");
        }

        [Test]
        public async Task CanUpdateOverheatingStatus()
        {
            _provider.SetOverheatingUsers("1", "2", "3");

            var rateLimiter = ObjectFactory.GetInstance<IActivityExecutionRateLimitingService>();

            _provider.SetOverheatingResults(new OverheatingUsersUpdateResults(new string[] {"4", "5"}, new string[] { "1", "2" }));

            await Task.Delay(2000);

            Assert.IsTrue(rateLimiter.CheckActivityExecutionRate("1"), "User \"1\" should not be banned");
            Assert.IsTrue(rateLimiter.CheckActivityExecutionRate("2"), "User \"2\" should not be banned");
            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("4"), "User \"4\" should be banned");
            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("5"), "User \"5\" should be banned");
            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("3"), "User \"3\" should be banned");

            Assert.AreEqual(1, _provider.GetOverheatingUsersCount, "Should load overheating users from the provider only once");
        }
    }
}
