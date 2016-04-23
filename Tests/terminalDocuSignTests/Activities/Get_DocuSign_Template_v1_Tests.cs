using System.ComponentModel.DataAnnotations;
using System.Linq;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign;
using terminalDocuSign.Activities;
using terminalDocuSign.Activities;
using terminalDocuSign.Services.New_Api;
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
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = new Get_DocuSign_Template_v1();
            var result = target.ValidateActivityInternal(FixtureData.TestActivity1());
            Assert.AreNotEqual(ValidationResult.Success, result);
            Assert.AreEqual(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage, result.ErrorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenThereAreNoTemplates_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = new Get_DocuSign_Template_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            var result = target.ValidateActivityInternal(activityDO);
            Assert.AreNotEqual(ValidationResult.Success, result);
            Assert.AreEqual(DocuSignValidationUtils.NoTemplateExistsErrorMessage, result.ErrorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenTemplateIsNotSelected_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = new Get_DocuSign_Template_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            var result = target.ValidateActivityInternal(activityDO);
            Assert.AreNotEqual(ValidationResult.Success, result);
            Assert.AreEqual(DocuSignValidationUtils.TemplateIsNotSelectedErrorMessage, result.ErrorMessage);
        }
        [Test]
        public void ActivityIsValid_WhenTemplatetIsSelected_ReturnsTrue()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = new Get_DocuSign_Template_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SelectTemplate(activityDO);
            var result = target.ValidateActivityInternal(activityDO);
            Assert.AreEqual(ValidationResult.Success, result);
        }

        private void SelectTemplate(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                var templateList = configControls.Controls.OfType<DropDownList>().FirstOrDefault();
                templateList.selectedKey = "First";
            }
        }
    }
}
