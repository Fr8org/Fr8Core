using System.Configuration;
using NUnit.Framework;
using pluginAzureSqlServer.Actions;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using pluginDocuSign.Actions;
using Data.Interfaces.DataTransferObjects;
using System.Linq;
using System.Collections.Generic;
using pluginDocuSign.Controllers;
using System.Web.Http.Results;

namespace DockyardTest.Actions
{
    [TestFixture]
    [Category("Plugin.DocuSign.WaitForDocuSignEventV1")]
    public class WaitForDocuSignEventV1 : BaseTest
    {
        Wait_For_DocuSign_Event_v1 pluginAction;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            pluginAction = new Wait_For_DocuSign_Event_v1();
        }

        [Test]
        public void InitialConfigurationResponse_ShouldAddCrates()
        {
            var action = FixtureData.TestAction1();
            CrateStorageDTO result = (CrateStorageDTO) pluginAction.Configure(action);

            Assert.AreEqual(5, result.CratesDTO.Count);
            Assert.IsTrue(result.CratesDTO.Any(c => c.Label == "Selected_DocuSign_Template"));
            Assert.IsTrue(result.CratesDTO.Any(c => c.Label == "Event_Envelope_Sent"));
            Assert.IsTrue(result.CratesDTO.Any(c => c.Label == "Event_Envelope_Received"));
            Assert.IsTrue(result.CratesDTO.Any(c => c.Label == "Event_Recipient_Signed"));
            Assert.IsTrue(result.CratesDTO.Any(c => c.Label == "Event_Recipient_Sent"));

            string json = result.CratesDTO.SingleOrDefault(c => c.Label == "Selected_DocuSign_Template").Contents;
            var field = Newtonsoft.Json.JsonConvert.DeserializeObject<FieldDefinitionDTO>(json);
            var events = field.Events;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual("requestConfiguration", events[0].Handler);
            Assert.AreEqual("onSelect", events[0].Name);
       }

        [Test]
        public void FollowupConfigurationResponse_ShouldAddCrates()
        {
            var action = FixtureData.WaitForDocuSignEvent_Action();
            CrateStorageDTO result = (CrateStorageDTO)pluginAction.Configure(action, true);
            List<FieldDefinitionDTO> fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(result.CratesDTO[1].Contents);
            Assert.AreEqual(4, fields.Count);
        }

        [Test]
        public void ActionTemplateController_ShouldReturnActionTemplate()
        {
            var controller = new ActionTemplateController();
            var response = controller.Configure();
            var result = (response as OkNegotiatedContentResult<ActionTemplateDTO>).Content;
            Assert.AreEqual("localhost:53234", result.DefaultEndPoint);
            Assert.AreEqual("1.0", result.Version);
            Assert.AreEqual("Wait For DocuSign Event", result.Name);
        }
    }
}