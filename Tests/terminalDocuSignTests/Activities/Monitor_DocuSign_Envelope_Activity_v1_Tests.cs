using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;
using terminalDocuSignTests.Fixtures;
using Fr8.Testing.Unit.Fixtures;

namespace terminalDocuSignTests.Activities
{
    [TestFixture]
    [Category("Monitor_DocuSign_Envelope_Activity_v1")]
    public class Monitor_DocuSign_Envelope_Activity_v1_Tests : BaseTest
    {
        [Test]
        public async Task ActivityIsValid_WhenIsNotConfigured_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = New<Monitor_DocuSign_Envelope_Activity_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            var result = await Validate(target, activityContext); 
            Assert.AreNotEqual(ValidationResult.Success, result);

            AssertErrorMessage(result, DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenNoNotificationIsSelected_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = New<Monitor_DocuSign_Envelope_Activity_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);
            SetRecipientConditionSelected(activityContext.ActivityPayload);
            SetRecipientText(activityContext.ActivityPayload);
            var result = await Validate(target, activityContext);

            AssertErrorMessage(result, "At least one notification option must be selected");
        }

        [Test]
        public async Task ActivityIsValid_WhenNoEnvelopeConditionIsSelected_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = New<Monitor_DocuSign_Envelope_Activity_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);
            SetNotificationSelected(activityContext.ActivityPayload);
            var result = await Validate(target, activityContext);

            AssertErrorMessage(result, "At least one envelope option must be selected");
        }

        [Test]
        public async Task ActivityIsValid_WhenTemplateMustBeSetButThereAreNoTemplates_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = New<Monitor_DocuSign_Envelope_Activity_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);
            SetNotificationSelected(activityContext.ActivityPayload);
            SetTemplateConditionSelected(activityContext.ActivityPayload);
            var result = await Validate(target, activityContext);

            AssertErrorMessage(result, DocuSignValidationUtils.NoTemplateExistsErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenTemplateMustBeSetButItIsNot_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = New<Monitor_DocuSign_Envelope_Activity_v1>();
            var activityDO = FixtureData.TestActivity1();
            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);
            SetNotificationSelected(activityContext.ActivityPayload);
            SetTemplateConditionSelected(activityContext.ActivityPayload);
            var result = await Validate(target, activityContext);

            AssertErrorMessage(result, DocuSignValidationUtils.TemplateIsNotSelectedErrorMessage);
        }
        [Test]
        public async Task ActivityIsValid_WhenAllFieldsAreSet_ReturnsTrue()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = New<Monitor_DocuSign_Envelope_Activity_v1>();
            var activityDO = FixtureData.TestActivity1();
            var activityContext = FixtureData.TestActivityContext1();
            await target.Configure(activityContext);
            SetNotificationSelected(activityContext.ActivityPayload);
            SetTemplateConditionSelected(activityContext.ActivityPayload);
            SetTemplate(activityContext.ActivityPayload);
            var result = await Validate(target, activityContext);
            Assert.AreEqual(null, result);
        }

        private void SetNotificationSelected(ActivityPayload activity)
        {
            var configControls = activity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
            configControls.Controls
                .Where(c => c.Type == ControlTypes.CheckBox)
                .First(x => x.Name == "RecipientSigned")
                .Selected = true;
        }

        private void SetRecipientConditionSelected(ActivityPayload activity)
        {
            var configControls = activity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
            configControls.Controls
                            .OfType<RadioButtonGroup>()
                            .First()
                            .Radios
                            .First(x => x.Name == "recipient")
                            .Selected = true;
        }

        private void SetRecipientText(ActivityPayload activity)
        {
            var configControls = activity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
            configControls.Controls
                            .OfType<RadioButtonGroup>()
                            .First()
                            .Radios
                            .First(x => x.Name == "recipient")
                            .Controls
                            .First()
                            .Value = "foo@bar.com";
        }

        private void SetTemplateConditionSelected(ActivityPayload activity)
        {
            var configControls = activity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
            configControls.Controls
                            .OfType<RadioButtonGroup>()
                            .First()
                            .Radios
                            .First(x => x.Name == "template")
                            .Selected = true;
        }

        private void SetTemplate(ActivityPayload activity)
        {
            var configControls = activity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
            configControls.Controls
                            .OfType<RadioButtonGroup>()
                            .First()
                            .Radios
                            .First(x => x.Name == "template")
                            .Controls
                            .OfType<DropDownList>()
                            .First()
                            .selectedKey = "First";
        }
    }
}
