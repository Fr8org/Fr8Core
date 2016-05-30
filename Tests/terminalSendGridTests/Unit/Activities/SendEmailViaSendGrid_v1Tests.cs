using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using NUnit.Framework;
using SendGrid;
using StructureMap;
using terminalSendGrid;
using terminalSendGrid.Activities;
using terminalSendGridTests.Fixtures;
using terminalUtilities.Interfaces;
using terminalUtilities.SendGrid;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Models;
using Utilities;
using UtilitiesTesting;

namespace terminalSendGridTests.Unit.Activities
{
    [TestFixture]
    [Category("terminalSendGrid")]
    public class SendEmailViaSendGrid_v1Tests : BaseTest
    {
        private SendEmailViaSendGrid_v1 _gridActivity;
        private ICrateManager _crate;
        private ActivityPayload activityPayload;

        public override void SetUp()
        {
            base.SetUp();
            ObjectFactory.Configure(x => x.AddRegistry<TerminalSendGridStructureMapBootstrapper.LiveMode>());
            ObjectFactory.Configure(cfg => cfg.For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>())));
            ObjectFactory.Configure(cfg => cfg.For<IEmailPackager>().Use(new SendGridPackager()));
            TerminalBootstrapper.ConfigureTest();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            activityPayload = GetActivityResult().Result;
        }

        [Test]
        public void Configure_ReturnsCrateDTO()
        {
            // Act
            var controlsCrates = activityPayload.CrateStorage;
            // Assert
            Assert.IsNotNull(controlsCrates);
            Assert.AreEqual(controlsCrates.Count(), 1);
        }

        [Test]
        public void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            // Act
            var controlsCrate = activityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            // Assert
            Assert.IsNotNull(controlsCrate);
            Assert.IsNotNull(controlsCrate.ManifestType);
            Assert.IsNotNull(controlsCrate.Content);
            Assert.AreEqual(controlsCrate.Content.Controls.Count, 3);
        }

        [Test]
        public void Configure_ReturnsEmailControls()
        {
            // Act && Assert
            var standardControls = activityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(standardControls);

            var controls = standardControls.Controls.Count(a => a.Type == "TextSource");

            Assert.AreEqual(3, controls);
        }

        private async Task<ActivityPayload> GetActivityResult()
        {
            _gridActivity = New<SendEmailViaSendGrid_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            await _gridActivity.Configure(activityContext);
            return activityContext.ActivityPayload;
        }

        [Test]
        public async Task Run_Shouldnt_Throw_Exception()
        {
            // Arrange
            ICrateManager Crate = ObjectFactory.GetInstance<ICrateManager>();
            _gridActivity = New<SendEmailViaSendGrid_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            var executionContext = FixtureData.CrateExecutionContextForSendEmailViaSendGridConfiguration;
            //updating controls
            var standardControls = activityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            foreach (TextSource control in standardControls.Controls)
            {
                control.ValueSource = "specific";
                control.Value = (control.Name == "EmailAddress") ? "test@mail.com" : "test";
            }
            var crate = Crate.CreateStandardConfigurationControlsCrate(BaseTerminalActivity.ConfigurationControlsLabel, standardControls.Controls.ToArray());
            activityContext.ActivityPayload.CrateStorage.Add(crate);
            // Act
            await _gridActivity.Run(activityContext, executionContext);
        }
    }
}
