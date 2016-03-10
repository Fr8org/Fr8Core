using System.Linq;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;
using terminalDocuSign.Actions;
using terminalDocuSignTests.Fixtures;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSignTests.Activities
{
    [TestFixture]
    [Category("Get_DocuSign_Template_v1")]
    public class Get_DocuSign_Template_v1_Tests : BaseTest
    {
        [Test]
        public void ActivityIsValid_WhenIsNotConfigured_ReturnsFalse()
        {
            var target = new Get_DocuSign_Template_v1();
            string errorMessage;
            var result = target.ActivityIsValid(FixtureData.TestActivity1(), out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("Controls are not configured properly", errorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenThereAreNoTemplates_ReturnsFalse()
        {
            var target = new Get_DocuSign_Template_v1(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("Please link at least one template to your DocuSign account", errorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenTemplateIsNotSelected_ReturnsFalse()
        {
            var target = new Get_DocuSign_Template_v1(DocuSignActivityFixtureData.DocuSignManagerWithTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("Template is not selected", errorMessage);
        }
        [Test]
        public void ActivityIsValid_WhenTemplatetIsSelected_ReturnsTrue()
        {
            var target = new Get_DocuSign_Template_v1(DocuSignActivityFixtureData.DocuSignManagerWithTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SelectTemplate(activityDO);
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsTrue(result);
        }

        private void SelectTemplate(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                var templateList = configControls.Controls.OfType<DropDownList>().FirstOrDefault();
                templateList.selectedKey = "1";
            }
        }
    }
}
