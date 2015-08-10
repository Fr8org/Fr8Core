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
        private const string SchemaJson =
            @"{'title': 'available actions','type':'object','properties':{'typename':{'type':'string'},'version':{'type':'number'}}}";

        [Test]
        [Category("Controllers.ActionController.GetAvailable")]
        public void ActionController_GetAvailable_JsonStringShouldBeValid()
        {
            var controller = new ActionController();
            var actionsString = controller.GetAvailable();

            var schema = JsonSchema.Parse(SchemaJson);
            var actions = JObject.Parse(actionsString);

            IList<string> messages;
            var valid = actions.IsValid(schema, out messages);

            Assert.IsTrue(valid);
        }
    }
}