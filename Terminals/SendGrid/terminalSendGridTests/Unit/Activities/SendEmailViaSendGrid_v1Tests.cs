using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Models;
using NUnit.Framework;
using SendGrid;
using StructureMap;
using terminalSendGrid;
using terminalSendGrid.Activities;
using terminalSendGridTests.Fixtures;
using terminalUtilities.Interfaces;
using terminalUtilities.SendGrid;
using Fr8.Testing.Unit;

namespace terminalSendGridTests.Unit.Activities
{
    [TestFixture]
    [Category("terminalSendGrid")]
    public class SendEmailViaSendGrid_v1Tests : BaseTest
    {
        private Send_Email_Via_SendGrid_v1 _gridActivity;
        private ICrateManager _crate;
        private ActivityPayload activityPayload;

        public override void SetUp()
        {
            base.SetUp();
            ObjectFactory.Configure(x => x.AddRegistry<TerminalSendGridStructureMapBootstrapper.LiveMode>());
            ObjectFactory.Configure(cfg => cfg.For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>())));
            ObjectFactory.Configure(cfg => cfg.For<IEmailPackager>().Use(c => new SendGridPackager(c.GetInstance<IConfigRepository>())));
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
            _gridActivity = New<Send_Email_Via_SendGrid_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            await _gridActivity.Configure(activityContext);
            return activityContext.ActivityPayload;
        }

        [Test]
        public async Task Run_Shouldnt_Throw_Exception()
        {
            // Arrange
            ICrateManager Crate = ObjectFactory.GetInstance<ICrateManager>();
            _gridActivity = New<Send_Email_Via_SendGrid_v1>();
            var activityContext = FixtureData.TestActivityContext1();
            var executionContext = FixtureData.CrateExecutionContextForSendEmailViaSendGridConfiguration;
            //updating controls
            var standardControls = activityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            foreach (TextSource control in standardControls.Controls)
            {
                control.ValueSource = "specific";
                control.Value = (control.Name == "EmailAddress") ? "test@mail.com" : "test";
            }
            activityContext.ActivityPayload.CrateStorage.Add(TerminalActivityBase.ConfigurationControlsLabel, standardControls);

            // Act
            await _gridActivity.Run(activityContext, executionContext);
        }
    }
}
