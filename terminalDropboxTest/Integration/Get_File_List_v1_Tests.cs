using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalDropboxTests.Fixtures;

namespace terminalDropboxTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Get_File_List_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDropbox"; }
        }

        [Test, CategoryAttribute("Integration.terminalDropbox")]
        public async Task GetFiles_Run_ReturnsPayload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            var requestActionDTO = HealthMonitor_FixtureData.GetFileListTestActionDTO();
            AddOperationalStateCrate(requestActionDTO, new OperationalStateCM());

            //Act
            var payloadDTOResult = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, requestActionDTO);
            var jsonData = ((JValue)(payloadDTOResult.CrateStorage.Crates[1].Contents)).Value.ToString();
            var dropboxFileList = JsonConvert.DeserializeObject<List<string>>(jsonData);

            // Assert
            Assert.NotNull(payloadDTOResult);
            Assert.True(dropboxFileList.Any());

        }
    }
}
