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
    [Category("Monitor_DocuSign_Envelope_Activity_v1")]
    public class Monitor_DocuSign_Envelope_Activity_v1_Tests : BaseTest
    {
        [Test]
        public void ActivityIsValid_WhenIsNotConfigured_ReturnsFalse()
        {
            var target = new Monitor_DocuSign_Envelope_Activity_v1();
            string errorMessage;
            var result = target.ActivityIsValid(FixtureData.TestActivity1(), out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("Controls are not configured properly", errorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenNoNotificationIsSelected_ReturnsFalse()
        {
            var target = new Monitor_DocuSign_Envelope_Activity_v1(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SetRecipientConditionSelected(activityDO);
            SetRecipientText(activityDO);
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("At least one notification checkbox must be checked.", errorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenNoEnvelopeConditionIsSelected_ReturnsFalse()
        {
            var target = new Monitor_DocuSign_Envelope_Activity_v1(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SetNotificationSelected(activityDO);
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("One option from the radio buttons must be selected.", errorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenTemplateMustBeSetButThereAreNoTemplates_ReturnsFalse()
        {
            var target = new Monitor_DocuSign_Envelope_Activity_v1(DocuSignActivityFixtureData.DocuSignManagerWithoutTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SetNotificationSelected(activityDO);
            SetTemplateConditionSelected(activityDO);
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("Please link at least one template to your DocuSign account", errorMessage);
        }

        [Test]
        public void ActivityIsValid_WhenTemplateMustBeSetButItIsNot_ReturnsFalse()
        {
            var target = new Monitor_DocuSign_Envelope_Activity_v1(DocuSignActivityFixtureData.DocuSignManagerWithTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SetNotificationSelected(activityDO);
            SetTemplateConditionSelected(activityDO);
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsFalse(result);
            Assert.AreEqual("Template is not selected", errorMessage);
        }
        [Test]
        public void ActivityIsValid_WhenAllFieldsAreSet_ReturnsTrue()
        {
            var target = new Monitor_DocuSign_Envelope_Activity_v1(DocuSignActivityFixtureData.DocuSignManagerWithTemplates());
            var activityDO = FixtureData.TestActivity1();
            activityDO = target.Configure(activityDO, FixtureData.AuthToken_TerminalIntegration()).Result;
            SetNotificationSelected(activityDO);
            SetTemplateConditionSelected(activityDO);
            SetTemplate(activityDO);
            string errorMessage;
            var result = target.ActivityIsValid(activityDO, out errorMessage);
            Assert.IsTrue(result);
        }

        private void SetNotificationSelected(ActivityDO activity)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(activity))
            {
                var configControls = crateStorage.FirstCrate<StandardConfigurationControlsCM>(c => true).Content;
                configControls.Controls
                    .Where(c => c.Type == ControlTypes.CheckBox)
                    .First(x => x.Name == "Event_Recipient_Signed")
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
                              .Value = "Some text";
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
                              .Value = "First";
            }
        }
    }
}
