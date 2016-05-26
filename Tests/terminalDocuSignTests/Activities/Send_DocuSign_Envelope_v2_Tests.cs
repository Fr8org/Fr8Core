using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Actions;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Helpers;
using TerminalBase.Helpers;
using TerminalBase.Models;

namespace terminalDocuSignTests.Activities
{
    [TestFixture]
    public class Send_DocuSign_Envelope_v2_Tests : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            var docuSignManagerMock = ObjectFactory.GetInstance<Mock<IDocuSignManager>>();
            docuSignManagerMock.Setup(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()))
                         .Returns(new List<FieldDTO> { new FieldDTO("First", "1") });
            docuSignManagerMock.Setup(x => x.GetTemplateRecipientsTabsAndDocuSignTabs(It.IsAny<DocuSignApiConfiguration>(), "1"))
                               .Returns(new Tuple<IEnumerable<FieldDTO>, IEnumerable<DocuSignTabDTO>>(
                                            new[]
                                            {
                                                new FieldDTO(DocuSignConstants.DocuSignRoleEmail) { Tags = DocuSignConstants.DocuSignSignerTag },
                                                new FieldDTO(DocuSignConstants.DocuSignRoleName) { Tags = DocuSignConstants.DocuSignTabTag },
                                                new FieldDTO("RadioName") { Tags = DocuSignConstants.DocuSignTabTag },
                                                new FieldDTO("DropDownListName") { Tags = DocuSignConstants.DocuSignTabTag },
                                                new FieldDTO("CheckBoxName") { Tags = DocuSignConstants.DocuSignTabTag }
                                            },
                                            new[]
                                            {
                                                new DocuSignTabDTO { Fr8DisplayType = ControlTypes.TextBox, Name = DocuSignConstants.DocuSignRoleName },
                                                new DocuSignMultipleOptionsTabDTO
                                                {
                                                    Fr8DisplayType = ControlTypes.RadioButtonGroup,
                                                    Name = "RadioName",
                                                    Items = new List<DocuSignOptionItemTabDTO> { new DocuSignOptionItemTabDTO { Text = "Text", Value = "Value" } }
                                                },
                                                 new DocuSignMultipleOptionsTabDTO
                                                {
                                                    Fr8DisplayType = ControlTypes.DropDownList,
                                                    Name = "DropDownListName",
                                                    Items = new List<DocuSignOptionItemTabDTO> { new DocuSignOptionItemTabDTO { Text = "Text", Value = "Value" } }
                                                },
                                                 new DocuSignTabDTO { Fr8DisplayType = ControlTypes.CheckBox, Name = "CheckBoxName" }
                                            }));
        }

        [Test]
        public async Task Initialize_Always_LoadsDocuSignTemplates()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var context = new ActivityContext
            {
                ActivityPayload = new ActivityPayload { CrateStorage = new CrateStorage()},
                AuthorizationToken = FakeToken
            };
            await activity.Configure(context);
            ObjectFactory.GetInstance<Mock<IDocuSignManager>>().Verify(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()), Times.Once(), "Template list was not loaded from DosuSign");
        }

        [Test]
        public async Task Configure_Always_ReloadsDocuSignTemplates()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var context = new ActivityContext
            {
                ActivityPayload = new ActivityPayload { CrateStorage = new CrateStorage() },
                AuthorizationToken = FakeToken
            };
            await activity.Configure(context);
            await activity.Configure(context);
            ObjectFactory.GetInstance<Mock<IDocuSignManager>>().Verify(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()), Times.Exactly(2), "Template list was not reloaded from DosuSign");
        }

        [Test]
        public async Task Configure_Always_MapsDocuSignTemplateFieldsToControls()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var context = new ActivityContext
            {
                ActivityPayload = new ActivityPayload { CrateStorage = new CrateStorage() },
                AuthorizationToken = FakeToken
            };
            await activity.Configure(context);

            context.ActivityPayload.CrateStorage.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x => x.TemplateSelector.SelectByValue("1"));
            await activity.Configure(context);
            var activityUi = context.ActivityPayload.CrateStorage.GetReadonlyActivityUi<Send_DocuSign_Envelope_v2.ActivityUi>();
            Assert.AreEqual(1, activityUi.RolesFields.Count, "Incorrect number of controls were generated for role fields");
            Assert.AreEqual(1, activityUi.TextFields.Count, "Incorrect number of controls were generated for text fields");
            Assert.AreEqual(1, activityUi.CheckBoxFields.Count, "Incorrect number of controls were generated for check box fields");
            Assert.AreEqual(1, activityUi.RadioButtonGroupFields.Count, "Incorrect number of controls were generated for radio group fields");
            Assert.AreEqual(1, activityUi.DropDownListFields.Count, "Incorrect number of controls were generated for list fields");
        }

        [Test]
        public async Task Activate_WhenTemplateIsNotSelected_FailsValidation()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var context = new ActivityContext
            {
                ActivityPayload = new ActivityPayload { CrateStorage = new CrateStorage() },
                AuthorizationToken = FakeToken
            };
            await activity.Configure(context);
            await activity.Activate(context);
            var storage = context.ActivityPayload.CrateStorage;
            var validationCrate = storage.FirstCrateOrDefault<ValidationResultsCM>();
            Assert.IsNotNull(validationCrate, "Validation crate was not found in activity storage");
            Assert.AreEqual(1, validationCrate.Content.ValidationErrors.Count, "Incorrect number of validation errors");
            Assert.AreEqual(1, validationCrate.Content.GetErrorsForControl(nameof(Send_DocuSign_Envelope_v2.ActivityUi.TemplateSelector)).Count, "Incorrect number of validation error for template selector");
        }

        [Test]
        public async Task Activate_WhenRoleEmailValueIsIncorrect_FailsValidation()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var context = new ActivityContext
            {
                ActivityPayload = new ActivityPayload { CrateStorage = new CrateStorage() },
                AuthorizationToken = FakeToken
            };
            await activity.Configure(context);
            context.ActivityPayload.CrateStorage.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x => x.TemplateSelector.SelectByValue("1"));
            await activity.Configure(context);
            await activity.Activate(context);
            var storage = context.ActivityPayload.CrateStorage;
            var validationCrate = storage.FirstCrateOrDefault<ValidationResultsCM>();
            Assert.IsNotNull(validationCrate, "Validation crate was not found in activity storage");
            Assert.AreEqual(1, validationCrate.Content.ValidationErrors.Count, "Incorrect number of validation errors");
            Assert.AreEqual(1, validationCrate.Content.GetErrorsForControl($"{nameof(Send_DocuSign_Envelope_v2.ActivityUi.RolesFields)}_{DocuSignConstants.DocuSignRoleEmail}").Count, "Incorrect number of validation error for role email control");
        }

        [Test]
        public async Task Run_WhenValidationSucceeds_MapsControlsValuesToDocuSignTemplateFieldsAndSendEmail()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var context = new ActivityContext
            {
                ActivityPayload = new ActivityPayload { CrateStorage = new CrateStorage() },
                AuthorizationToken = FakeToken
            };
            await activity.Configure(context);
            context.ActivityPayload.CrateStorage.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x => x.TemplateSelector.SelectByValue("1"));
            await activity.Configure(context);
            context.ActivityPayload.CrateStorage.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x =>
                                                                                 {
                                                                                     x.CheckBoxFields[0].Selected = true;
                                                                                     x.DropDownListFields[0].SelectByKey("Value");
                                                                                     x.RadioButtonGroupFields[0].Radios[0].Selected = true;
                                                                                     x.RolesFields[0].TextValue = "xx@xx.xx";
                                                                                     x.TextFields[0].TextValue = "value";
                                                                                 });
            ObjectFactory.GetInstance<Mock<IDocuSignManager>>().Setup(x => x.SendAnEnvelopeFromTemplate(It.IsAny<DocuSignApiConfiguration>(), It.IsAny<List<FieldDTO>>(), It.IsAny<List<FieldDTO>>(), It.IsAny<string>(), It.IsAny<StandardFileDescriptionCM>()))
                         .Callback<DocuSignApiConfiguration, List<FieldDTO>, List<FieldDTO>, string, StandardFileDescriptionCM>(AssertEnvelopeParameters);
            await activity.Run(context, new ContainerExecutionContext() {ContainerId = Guid.NewGuid(), PayloadStorage = new CrateStorage(Crate.FromContent("", new OperationalStateCM()))});
            ObjectFactory.GetInstance<Mock<IDocuSignManager>>().Verify(x => x.SendAnEnvelopeFromTemplate(It.IsAny<DocuSignApiConfiguration>(), It.IsAny<List<FieldDTO>>(), It.IsAny<List<FieldDTO>>(), It.IsAny<string>(), null),
                                                                       Times.Once(),
                                                                       "Run didn't send DocuSign envelope");

        }

        private void AssertEnvelopeParameters(DocuSignApiConfiguration loginInfo, List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId, StandardFileDescriptionCM file)
        {
            Assert.AreEqual("xx@xx.xx", rolesList[0].Value, "Incorrect value for role email field");
            Assert.AreEqual("Value", fieldList.First(x => x.Key == "DropDownListName").Value, "Incorrect value for list field");
            Assert.AreEqual("Value", fieldList.First(x => x.Key == "RadioName").Value, "Incorrect value for radio field");
            Assert.AreEqual("value", fieldList.First(x => x.Key == DocuSignConstants.DocuSignRoleName).Value, "Incorrect value for text field");
            Assert.AreEqual("true", fieldList.First(x => x.Key == "CheckBoxName").Value, "Incorrect value for checkbox field");
        }
    }
}
