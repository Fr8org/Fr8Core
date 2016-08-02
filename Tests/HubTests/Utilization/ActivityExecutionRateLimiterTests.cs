using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Utilization;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Utilization
{
    [TestFixture]
    [Category("UtilizationMonitoring")]
    public class ActivityExecutionRateLimiterTests : BaseTest
    {
        private readonly ManualyTriggeredTimerService _timerService = new ManualyTriggeredTimerService();

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

            public override OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold, TimeSpan metricReportValidTime, TimeSpan banTime)
            {
                if (_readyResults != null)
                {
                    var result = _readyResults;
                    _readyResults = null;

                    return result;
                }

                return base.UpdateOverheatingUsers(threshold, metricReportValidTime, banTime);
            }
        }

        public class PusherMock : IPusherNotifier
        {
            public readonly List<NotificationMessageDTO> Notifications = new List<NotificationMessageDTO>();

            public string GetChanelMessages(string email)
            {
                throw new NotImplementedException();
            }

            public void NotifyUser(NotificationMessageDTO notificationMessage, string userId)
            {
                Notifications.Add(notificationMessage);
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
            ObjectFactory.Container.Inject<ITimer>(_timerService);
        }

        public override void TearDown()
        {
            _timerService.Clear();
            base.TearDown();
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
        public void CanUpdateOverheatingStatus()
        {
            _provider.SetOverheatingUsers("1", "2", "3");

            var rateLimiter = ObjectFactory.GetInstance<IActivityExecutionRateLimitingService>();

            _provider.SetOverheatingResults(new OverheatingUsersUpdateResults(new [] {"4", "5"}, new [] { "1", "2" }));

            _timerService.Tick();

            Assert.IsTrue(rateLimiter.CheckActivityExecutionRate("1"), "User \"1\" should not be banned");
            Assert.IsTrue(rateLimiter.CheckActivityExecutionRate("2"), "User \"2\" should not be banned");
            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("4"), "User \"4\" should be banned");
            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("5"), "User \"5\" should be banned");
            Assert.IsFalse(rateLimiter.CheckActivityExecutionRate("3"), "User \"3\" should be banned");

            Assert.AreEqual(1, _provider.GetOverheatingUsersCount, "Should load overheating users from the provider only once");
        }

        [Test]
        public void CanNotifyUser()
        {
            Fr8AccountDO user1;
            Fr8AccountDO user2;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                user1 = FixtureData.TestUser1();
                user1.UserName = "user1";

                uow.UserRepository.Add(user1);

                user2 = FixtureData.TestUser2();
                user2.UserName = "user2";

                uow.UserRepository.Add(user2);

                uow.SaveChanges();
            }

            _provider.SetOverheatingUsers(user1.Id);

            var pusherMock = new PusherMock();

            ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock);

            var rateLimiter = ObjectFactory.GetInstance<IActivityExecutionRateLimitingService>(); // to trigger service initialization

            _provider.SetOverheatingResults(new OverheatingUsersUpdateResults(new [] { user2.Id }, new [] { user1.Id }));

            _timerService.Tick();

            Assert.AreEqual(1, pusherMock.Notifications.Count, "Invalid number of push notifications");
            Assert.IsTrue(pusherMock.Notifications[0].Message.Contains("You are running more Activities than your capacity right now."), "Unexpected notification message");
        }
    }
}
