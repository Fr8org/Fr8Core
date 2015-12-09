using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;
using terminalTwilioTests.Fixture;

namespace terminalTwilioTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Send_Via_Twilio_v1Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalTwilio"; }
        }
        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalTwilio")]
        public async void Send_Via_Twilio_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalRunUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;
            //Act
            await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                    );

            //Assert
            //Assert.NotNull(responseActionDTO);
            //Assert.NotNull(responseActionDTO.CrateStorage);
            //Assert.NotNull(responseActionDTO.CrateStorage.Crates);

        }


    }
}
