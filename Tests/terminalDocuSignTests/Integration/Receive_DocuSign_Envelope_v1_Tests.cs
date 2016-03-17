using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalDocuSignTests.Fixtures;
using Hub.Managers;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Receive_DocuSign_Envelope_v1_Tests : BaseTerminalIntegrationTest
    {
        ActivityDTO activityDTODesignFields;

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }


        /// <summary>
        /// Validate if no upstream data is available only standard configuration controls is added
        /// </summary>
        [Test, Category("Integration.terminalDocuSign")]
        public async Task Receive_DocuSign_Envelope_Initial_Configuration_Check_Crate_Structure_Without_Upstream()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault());

            var textField = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            Assert.IsNotNull(textField.Controls.Where(w => w.GetType() == typeof(TextBlock)).FirstOrDefault());
        }

        /// <summary>
        /// Validate if upstream data is available design field is added is configuration crates
        /// </summary>
        [Test, Category("Integration.terminalDocuSign")]
        public async Task Receive_DocuSign_Envelope_Initial_Configuration_Check_Crate_Structure_With_Upstream()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            
            List<FieldDTO> fieldDTO = new List<FieldDTO>();
            fieldDTO.Add(new FieldDTO() { Key = "TemplateId", Value = "6ef29903-e405-4a24-8b92-a3a3ae8d1824" });
            FieldDescriptionsCM standardDesignFieldsCM = new FieldDescriptionsCM();
            standardDesignFieldsCM.Fields = fieldDTO;

            base.AddUpstreamCrate<FieldDescriptionsCM>(dataDTO, standardDesignFieldsCM);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(2, crateStorage.Count);
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault());
            Assert.IsNotNull(crateStorage.CrateContentsOfType<FieldDescriptionsCM>().SingleOrDefault());

            //Assign result actiondto to be used in RUN
            activityDTODesignFields = responseActionDTO;
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test, Category("Integration.terminalDocuSign")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}",
            MatchType = MessageMatch.Contains
        )]
        public async Task Receive_DocuSign_Envelope_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            dataDTO.ActivityDTO.AuthToken = null;

            await HttpPostAsync<Fr8DataDTO, JToken>(configureUrl, dataDTO);
        }

        /// <summary>
        /// Test run-time for action from Monitor_DocuSign_FollowUp_Configuration_RecipientValue.
        /// </summary>
        [Test, Category("Integration.terminalDocuSign")]
        public async Task Receive_DocuSign_Envelope_Run_Withpayload()
        {
            var envelopeId = Guid.NewGuid().ToString();

            var runUrl = GetTerminalRunUrl();

            var activityDTO = activityDTODesignFields;
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            AddPayloadCrate(
                dataDTO,
                new EventReportCM()
                {
                    EventPayload = new CrateStorage()
                    {
                        Data.Crates.Crate.FromContent(
                            "EventReport",
                            new StandardPayloadDataCM(
                                new FieldDTO("EnvelopeId", envelopeId)
                            )
                        )
                    }
                }
            );
            
            var responsePayloadDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            //Assert
            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            Assert.AreEqual(1, crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "DocuSign Envelope Data").Count());

            var docuSignPayload = crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "DocuSign Envelope Data").Single();
        }

        [Test]
        public async Task Receive_DocuSign_Envelope_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

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

        [Test]
        public async Task Receive_DocuSign_Envelope_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

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
