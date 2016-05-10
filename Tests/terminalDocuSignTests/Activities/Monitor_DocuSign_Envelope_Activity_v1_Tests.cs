using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;
using terminalDocuSignTests.Fixtures;
using UtilitiesTesting.Fixtures;

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
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            var result = await Validate(target, FixtureData.TestActivity1()); 
            Assert.AreNotEqual(ValidationResult.Success, result);

            AssertErrorMessage(result, DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenNoNotificationIsSelected_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = await target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration());
            SetRecipientConditionSelected(activityDO);
            SetRecipientText(activityDO);
            var result = await Validate(target, activityDO);

            AssertErrorMessage(result, "At least one notification option must be selected");
        }

        [Test]
        public async Task ActivityIsValid_WhenNoEnvelopeConditionIsSelected_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = await target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration());
            SetNotificationSelected(activityDO);
            var result = await Validate(target, activityDO);

            AssertErrorMessage(result, "At least one envelope option must be selected");
        }

        [Test]
        public async Task ActivityIsValid_WhenTemplateMustBeSetButThereAreNoTemplates_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates()));
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = await target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration());
            SetNotificationSelected(activityDO);
            SetTemplateConditionSelected(activityDO);
            var result = await Validate(target, activityDO);

            AssertErrorMessage(result, DocuSignValidationUtils.NoTemplateExistsErrorMessage);
        }

        [Test]
        public async Task ActivityIsValid_WhenTemplateMustBeSetButItIsNot_ReturnsFalse()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = await target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration());
            SetNotificationSelected(activityDO);
            SetTemplateConditionSelected(activityDO);
            var result = await Validate(target, activityDO);

            AssertErrorMessage(result, DocuSignValidationUtils.TemplateIsNotSelectedErrorMessage);
        }
        [Test]
        public async Task ActivityIsValid_WhenAllFieldsAreSet_ReturnsTrue()
        {
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(DocuSignActivityFixtureData.DocuSignManagerWithTemplates()));
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            var activityDO = FixtureData.TestActivity1();
            activityDO = await target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration());
            SetNotificationSelected(activityDO);
            SetTemplateConditionSelected(activityDO);
            SetTemplate(activityDO);
            var result = await Validate(target, activityDO);
            Assert.AreEqual(false, result.HasErrors);
        }

        private void SetNotificationSelected(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                configControls.Controls
                    .Where(c => c.Type == ControlTypes.CheckBox)
                    .First(x => x.Name == "RecipientSigned")
                    .Selected = true;
            }
        }

        private void SetRecipientConditionSelected(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                configControls.Controls
                              .OfType<RadioButtonGroup>()
                              .First()
                              .Radios
                              .First(x => x.Name == "recipient")
                              .Selected = true;
            }
        }

        private void SetRecipientText(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                configControls.Controls
                              .OfType<RadioButtonGroup>()
                              .First()
                              .Radios
                              .First(x => x.Name == "recipient")
                              .Controls
                              .First()
                              .Value = "foo@bar.com";
            }
        }

        private void SetTemplateConditionSelected(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                configControls.Controls
                              .OfType<RadioButtonGroup>()
                              .First()
                              .Radios
                              .First(x => x.Name == "template")
                              .Selected = true;
            }
        }

        private void SetTemplate(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
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
}
