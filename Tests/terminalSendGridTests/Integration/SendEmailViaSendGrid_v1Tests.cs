using System.Linq;
using Fr8.Testing.Integration;
using NUnit.Framework;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using terminalSendGridTests.Fixtures;

namespace terminalSendGridTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class SendEmailViaSendGrid_v1Tests : BaseTerminalIntegrationTest
    {
        ActivityDTO activityDTOInit = new ActivityDTO();

        public override string TerminalName
        {
            get { return "terminalSendGrid"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalSendGrid")]
        public async Task SendEmailViaSendGrid_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            activityDTOInit = responseActionDTO;
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault());
            // There is no FieldDescriptionsCM in the create storage;
            // TODO: UPDATE TEST
            // Assert.AreEqual(1, crateStorage.CrateContentsOfType<FieldDescriptionsCM>().Count());

            var standardConfigurationControlCM = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>();
            Assert.AreEqual(1, standardConfigurationControlCM.Where(w => w.FindByName("EmailAddress") != null).Count());
            Assert.AreEqual(1, standardConfigurationControlCM.Where(w => w.FindByName("EmailSubject") != null).Count());
            Assert.AreEqual(1, standardConfigurationControlCM.Where(w => w.FindByName("EmailBody") != null).Count());
        }

        [Test, Category("Integration.terminalSendGrid")]
        public async Task SendEmailViaSendGrid_Run_Returns_Payload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            var dataDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_Fr8DataDTO();


            using (var updatableStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                updatableStorage.Add(CreateCrates());
            }

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            AddPayloadCrate(
               dataDTO,
               new StandardPayloadDataCM() { }
            );

            //Act
            var responsePayloadDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            //Assert
            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);

            var StandardPayloadDataCM = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.IsNotNull(StandardPayloadDataCM);
        }

        private Crate CreateCrates()
        {
            var control = new TextSource()
            {
                Name = "EmailAddress",
                ValueSource = "specific",
                Value = "test@mail.com"
            };

            var control2 = new TextSource()
            {
                Name = "EmailSubject",
                ValueSource = "specific",
                Value = "test subject"
            };

            var control3 = new TextSource()
            {
                Name = "EmailBody",
                ValueSource = "specific",
                Value = "test body"
            };

            return PackControlsCrate(control, control2, control3);
        }

        private Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            var controls = new StandardConfigurationControlsCM(controlsList);
            return Crate<StandardConfigurationControlsCM>.FromContent("Configuration_Controls", controls);
        }

        [Test, Category("Integration.terminalSendGrid")]
        public async Task SendEmailViaSendGrid_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test, Category("Integration.terminalSendGrid")]
        public async Task SendEmailViaSendGrid_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
