using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.States;
using Hub.Interfaces;
using UtilitiesTesting;

namespace HubTests.Services
{
    [TestFixture,Ignore]
    [Category("SubscriptionService")]
    public class SubscriptionServiceTests : BaseTest
    {
        private ISubscription _subscription;

        public override void SetUp()
        {
            base.SetUp();
            _subscription = ObjectFactory.GetInstance<ISubscription>();
        }

        [Test]
        public void CanRetrieveTerminalRegistrations()
        {
            /*
             * TODO: Subscription logic should be changed in V2
             */


            //const string unavailablePluginName = "UnavailablePlugin";
            //const string noAccessPluginName = "NoAccessPlugin";
            //const string userAccessPluginName = "AvailableWithUserAccessPlugin";
            //const string adminAccessPluginName = "AvailableWithAdminAccessPlugin";
            //var unavailablePluginRegistration = new Mock<IPluginRegistration>();
            //var noAccessPluginRegistration = new Mock<IPluginRegistration>();
            //var userAccessPluginRegistration = new Mock<IPluginRegistration>();
            //var adminAccessPluginRegistration = new Mock<IPluginRegistration>();
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(unavailablePluginRegistration.Object).Named(unavailablePluginName));
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(noAccessPluginRegistration.Object).Named(noAccessPluginName));
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(userAccessPluginRegistration.Object).Named(userAccessPluginName));
            //ObjectFactory.Configure(i => i.For<IPluginRegistration>().Use(adminAccessPluginRegistration.Object).Named(adminAccessPluginName));
            //var account = new DockyardAccountDO()
            //{
            //    Subscriptions = new List<SubscriptionDO>()
            //    {
            //        new SubscriptionDO()
            //        {
            //            AccessLevel = AccessLevel.None,
            //            Plugin = new PluginDO() {Name = noAccessPluginName}
            //        },
            //        new SubscriptionDO()
            //        {
            //            AccessLevel = AccessLevel.User,
            //            Plugin = new PluginDO() {Name = userAccessPluginName}
            //        },
            //        new SubscriptionDO()
            //        {
            //            AccessLevel = AccessLevel.Admin,
            //            Plugin = new PluginDO() {Name = adminAccessPluginName}
            //        },
            //    }
            //};
            //var result = _subscription.GetAuthorizedPlugins(account).ToList();
            //Assert.That(result.Contains(userAccessPluginRegistration.Object), "Plugin with User access level didn't return");
            //Assert.That(result.Contains(adminAccessPluginRegistration.Object), "Plugin with Admin access level didn't return");
            //Assert.That(!result.Contains(noAccessPluginRegistration.Object), "Plugin with None access level returned");
            //Assert.That(!result.Contains(unavailablePluginRegistration.Object), "Unavailable plugin returned");
        }
    }
}
