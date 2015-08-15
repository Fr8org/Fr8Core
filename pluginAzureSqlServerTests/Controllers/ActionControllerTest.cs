using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using pluginAzureSqlServer.Controllers;
using UtilitiesTesting;

namespace pluginAzureSqlServerTests.Controllers
{
    [TestFixture]
    public class ActionControllerTest : BaseTest
    {
        [Test]
        [Category("Controllers.ActionController.GetAvailable")]
        public void ActionController_CanGetAvailable()
        {
            var controller = new ActionController();
            var actions = controller.GetAvailable();
            Assert.IsNotNull(actions);
        }
    }
}