using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Actions;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;

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
                                                    Items = new List<DocuSignOptionItemTabDTO> { new DocuSignOptionItemTabDTO { Text = "Text", Value = "Value", Selected = true } }
                                                },
                                                 new DocuSignMultipleOptionsTabDTO
                                                {
                                                    Fr8DisplayType = ControlTypes.DropDownList,
                                                    Name = "DropDownListName",
                                                    Items = new List<DocuSignOptionItemTabDTO> { new DocuSignOptionItemTabDTO { Text = "Text", Value = "Value", Selected = true } }
                                                },
                                                 new DocuSignTabDTO { Fr8DisplayType = ControlTypes.CheckBox, Name = "CheckBoxName" }
                                            }));
        }

        [Test]
        public async Task Initialize_Always_LoadsDocuSignTemplates()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            await activity.Configure(new ActivityDO(), FakeToken);
            ObjectFactory.GetInstance<Mock<IDocuSignManager>>().Verify(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()), Times.Once(), "Template list was not loaded from DosuSign");
        }

        [Test]
        public async Task Configure_Always_ReloadsDocuSignTemplates()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var currentActivity = await activity.Configure(new ActivityDO(), FakeToken);
            currentActivity = await activity.Configure(currentActivity, FakeToken);
            ObjectFactory.GetInstance<Mock<IDocuSignManager>>().Verify(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()), Times.Exactly(2), "Template list was not reloaded from DosuSign");
        }

        [Test]
        public async Task Configure_Always_MapsControlsToDocuSignTemplateFields()
        {
            var activity = ObjectFactory.GetInstance<Send_DocuSign_Envelope_v2>();
            var currentActivity = await activity.Configure(new ActivityDO(), FakeToken);
            currentActivity.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x => x.TemplateSelector.SelectByValue("1"));
            currentActivity = await activity.Configure(currentActivity, FakeToken);
            var activityUi = currentActivity.GetReadonlyActivityUi<Send_DocuSign_Envelope_v2.ActivityUi>();
            Assert.AreEqual(1, activityUi.RolesFields.Count, "Incorrect number of controls were genrated for role fields");
            Assert.AreEqual(1, activityUi.TextFields.Count, "Incorrect number of controls were generated for text fields");
            Assert.AreEqual(1, activityUi.CheckBoxFields.Count, "Incorrect number of controls were generated for check box fields");
            Assert.AreEqual(1, activityUi.RadioButtonGroupFields.Count, "Incorrect number of controls were generated for radio group fields");
            Assert.AreEqual(1, activityUi.DropDownListFields.Count, "Incorrect number of controls were generated for list fields");
        }
    }
}
