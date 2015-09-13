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
        [Ignore("Requires update after v2 refactoring.")]
        public void InitialConfigurationResponse_ShouldAddCrates()
        {
            var action = FixtureData.TestAction1();
            var package = new ActionDataPackageDTO(AutoMapper.Mapper.Map<ActionDTO>(action), null);
            CrateStorageDTO result = (CrateStorageDTO) pluginAction.Configure(package);

            Assert.AreEqual(1, result.CratesDTO.Count);
            var fieds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(result.CratesDTO[0].Contents);
            Assert.IsTrue(fieds.Any(c => c.Name == "Selected_DocuSign_Template"));
            Assert.IsTrue(fieds.Any(c => c.Name == "Event_Envelope_Sent"));
            Assert.IsTrue(fieds.Any(c => c.Name == "Event_Envelope_Received"));
            Assert.IsTrue(fieds.Any(c => c.Name == "Event_Recipient_Signed"));
            Assert.IsTrue(fieds.Any(c => c.Name == "Event_Recipient_Sent"));

            var events = fieds.SingleOrDefault(c => c.Name == "Selected_DocuSign_Template").Events;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual("requestConfiguration", events[0].Handler);
            Assert.AreEqual("onSelect", events[0].Name);
       }

        [Test]
        public void FollowupConfigurationResponse_ShouldAddCrates()
        {
            var action = FixtureData.WaitForDocuSignEvent_Action();
            var actionDTO = new ActionDataPackageDTO(AutoMapper.Mapper.Map<ActionDTO>(action), null);
            CrateStorageDTO result = (CrateStorageDTO)pluginAction.Configure(actionDTO, true);
            List<FieldDefinitionDTO> fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(result.CratesDTO[1].Contents);
            Assert.AreEqual(10, fields.Count);
        }

        [Test]
        public void ActionTemplateController_ShouldReturnActionTemplateList()
        {
            var controller = new ActionTemplateController();
            var response = controller.Get();
            var actionTemplateList = (response as OkNegotiatedContentResult<List<ActionTemplateDTO>>).Content;
            var actionTemplate = actionTemplateList[0];
            Assert.AreEqual("localhost:53234", actionTemplate.DefaultEndPoint);
            Assert.AreEqual("1.0", actionTemplate.Version);
            Assert.AreEqual("Wait For DocuSign Event", actionTemplate.Name);
        }
    }
}