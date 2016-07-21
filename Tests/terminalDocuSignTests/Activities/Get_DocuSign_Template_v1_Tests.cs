using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign;
using terminalDocuSign.Services.New_Api;
using terminalDocuSignTests.Fixtures;
using Fr8.Testing.Unit.Fixtures;
using terminalDocuSign.Activities;

namespace terminalDocuSignTests.Activities
{
    [TestFixture]
    [Category("Get_DocuSign_Template_v1")]
    public class Get_DocuSign_Template_v1_Tests : BaseTest
    {
        [Test]
        public async Task ActivityIsValid_WhenIsNotConfigured_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = New<Get_DocuSign_Template_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            var result = await Validate(target, activityContext);
            AssertErrorMessage(result, DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenThereAreNoTemplates_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = New<Get_DocuSign_Template_v1>();
            var activityDO = FixtureData.TestActivity1();

            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);

            //var activityContext = FixtureData.TestActivityContext1();
            var result = await Validate(target, activityContext);

            AssertErrorMessage(result, DocuSignValidationUtils.NoTemplateExistsErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenTemplateIsNotSelected_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = New<Get_DocuSign_Template_v1>();
            var activityDO = FixtureData.TestActivity1();

            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);

            //var activityPayload = FixtureData.TestActivityContext1().ActivityPayload;
            var result = await Validate(target, activityContext);

            AssertErrorMessage(result, DocuSignValidationUtils.TemplateIsNotSelectedErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenTemplatetIsSelected_ReturnsTrue()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = New<Get_DocuSign_Template_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);

            SelectTemplate(activityContext.ActivityPayload);

            //var activityPayload = FixtureData.TestActivityContext1().ActivityPayload;
            var result = await Validate(target, activityContext);
            Assert.AreEqual(null, result);
        }

        private void SelectTemplate(ActivityPayload activity)
        {
            var configControls = activity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
            var templateList = configControls.Controls.OfType<DropDownList>().FirstOrDefault();
            templateList.selectedKey = "First";
        }
    }
}
