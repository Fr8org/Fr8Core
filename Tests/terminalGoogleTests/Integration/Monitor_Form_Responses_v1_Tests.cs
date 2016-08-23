using System.Linq;
using NUnit.Framework;
using Fr8.Testing.Integration;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Services;

namespace terminalGoogleTests.Integration
{
    [Explicit]
    [Category("terminalGoogleTests.Integration")]
    public class Monitor_Form_Responses_v1_Tests : BaseTerminalIntegrationTest
    {
        private string ActivityName = "Monitor_Form_Responses_v1";
        public override string TerminalName => "terminalGoogle";

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.Monitor_Form_Responses_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActivityDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            //Assert
            Assert.NotNull(responseActivityDTO, "Call to Initial configuration to " + ActivityName + " returns null.");
            Assert.NotNull(responseActivityDTO.CrateStorage, "Call to Initial configuration to " + ActivityName + " returns ActivityDTO with no CrateStorage.");

            var crateStorage = Crate.FromDto(responseActivityDTO.CrateStorage);
            Assert.AreEqual(3, crateStorage.Count);
            Assert.IsNotNull(crateStorage.FirstCrateOrDefault<CrateDescriptionCM>(x => x.Label == CrateSignaller.RuntimeCrateDescriptionsCrateLabel), "ActivityDTO storage doesn't contain crate with Runtime Crates Descriptions.");
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault(), "ActivityDTO storage doesn't contain crate with Standard Configuration Controls.");
            Assert.IsNotNull(crateStorage.CrateContentsOfType<EventSubscriptionCM>().SingleOrDefault(), "ActivityDTO storage doesn't contain crate with Event Subscription.");
        }

        /// <summary>
        /// Validate correct crate-storage CM structure.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Initial_Configuration_Check_CM_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActivityDTO = HealthMonitor_FixtureData.Monitor_Form_Responses_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActivityDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActivityDTO
                );

            //Assert
            var crateStorage = Crate.FromDto(responseActivityDTO.CrateStorage);
            var standardConfigurationControlsCM = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            var eventSubscriptionCM = crateStorage.CrateContentsOfType<EventSubscriptionCM>().SingleOrDefault();
            Assert.IsNotNull(standardConfigurationControlsCM, "ActivityDTO storage doesn't contain crate with Standard Configuration Controls.");
            Assert.IsNotNull(eventSubscriptionCM, "ActivityDTO storage doesn't contain crate with Event Subscription.");
            var dropdown = standardConfigurationControlsCM.Controls.FirstOrDefault(s => s.GetType() == typeof(DropDownList));
            Assert.IsNotNull(dropdown, "No Drop Down List Box in the Controls");
            Assert.AreEqual("Selected_Google_Form", dropdown.Name, "The Drop Down List Box control has incorrect Name value.");
            Assert.AreEqual(1, crateStorage.Count(s => s.Label == "Standard Event Subscriptions"), "Number of the crates with Standard Event Subscription is not one.");
        }

        /// <summary>
        /// Validate dropdownlist source contains google forms(pre-installed in users google drive)
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Initial_Configuration_Check_Source_Fields()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.Monitor_Form_Responses_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActivityDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            //Assert
            var crateStorage = Crate.FromDto(responseActivityDTO.CrateStorage);
            var controls = crateStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content;
            Assert.IsNotNull(controls, "Controls crate is missing");
            var control = controls.Controls.FirstOrDefault(x => x.Name == "Selected_Google_Form") as DropDownList;
            Assert.IsNotNull(control, "Select Form control is missing");
            Assert.Greater(control.ListItems.Count, 0, "No Google form were loaded into DDLB control");
        }
        /// <summary>
        /// This test covers the test that the Drop Down List Box gets updated on followup configuration
        /// if it was empty after initial configuration. It needs to handle the case when Form was uploaded
        /// after the initial configuration.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Followup_Configuration_Updates_DDLB()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var fixtureData = new HealthMonitor_FixtureData();
            var dataDTO = fixtureData.Monitor_Form_Responses_v1_Followup_Fr8DataDTO();
            var initialDDLB = GetDropDownListControl(dataDTO.ActivityDTO);
            Assert.AreEqual(0, initialDDLB.ListItems.Count(), "Initial configuration of the " + ActivityName + " contains drop down list box with some list items.");
            //initial configuration call
            var followupConfigurationActivityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                configureUrl,
                dataDTO
            );
            var afterFollowupDDLB = GetDropDownListControl(followupConfigurationActivityDTO);
            Assert.IsNotEmpty(afterFollowupDDLB.ListItems, "Call to Followup configuration of the " + ActivityName + " did not update the drop down list box.");
        }

        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var activateUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var dataDTO = fixture.Monitor_Form_Responses_v1_ActivateDeactivate_Fr8DataDTO();

            //Act
            var responseActivityDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    activateUrl,
                    dataDTO
                );

            //Assert
            Assert.IsNotNull(responseActivityDTO, "Call to Activate for " + ActivityName + " returned null.");
        }

        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var dataDTO = fixture.Monitor_Form_Responses_v1_ActivateDeactivate_Fr8DataDTO();

            //Act
            var responseActivityDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            //Assert
            Assert.IsNotNull(responseActivityDTO, "Call to Deactivate for " + ActivityName + " returned null.");
            Assert.IsNotNull(responseActivityDTO.CrateStorage, "ActivityDTO containes no CrateStorage");
        }

        /// <summary>
        /// Should throw exception if cannot extract any data from google form
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""Operational state crate is not found""}",
            MatchType = MessageMatch.Contains
            )]
        public async Task Monitor_Form_Responses_Run_WithInvalidPapertrailUrl_ShouldThrowException()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare the activity DTO with valid target URL
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var activityDTO = fixture.Monitor_Form_Responses_v1_Run_EmptyPayload();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            //Act
            await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        }


        /// <summary>
        /// Should return more than one payload fielddto for the response
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Form_Responses_Run_Returns_Payload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var activityDTO = fixture.Monitor_Form_Responses_v1_Run_ActivityDTO();

            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };

            AddPayloadCrate(
               dataDTO,
               new EventReportCM()
               {
                   EventPayload = new CrateStorage()
                   {
                        Fr8.Infrastructure.Data.Crates.Crate.FromContent(
                            "Response",
                            new StandardPayloadDataCM(
                                new KeyValueDTO("response", "key1=value1&key2=value2")
                            )
                        )
                   }
               }
            );
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            //Act
            var responsePayloadDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            //Assert
            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);

            var FieldDescriptionsCM = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.IsNotNull(FieldDescriptionsCM, "Call to Run of the " + ActivityName + " returned ActivityDTO with no crate of Standard Payload Data.");
            var fields = FieldDescriptionsCM.PayloadObjects.SelectMany(s => s.PayloadObject);
            Assert.Greater(fields.Count(), 0, "The number or fields in the Payload Data is zero");
        }

        private DropDownList GetDropDownListControl(ActivityDTO activityDTO)
        {
            var crateStorage = Crate.FromDto(activityDTO.CrateStorage);
            var controls = crateStorage.CratesOfType<StandardConfigurationControlsCM>().Single().Content.Controls;
            var ddlb = (DropDownList)controls.SingleOrDefault(c => c.Type == ControlTypes.DropDownList);
            return ddlb;
    }
    }
}
