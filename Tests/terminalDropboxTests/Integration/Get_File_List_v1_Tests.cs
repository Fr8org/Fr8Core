using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
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
    public class Get_File_List_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalDropbox";

        [Test, Category("Integration.terminalDropbox")]
        public async Task GetFileList_InitialConfig_ReturnsActivity()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            //Act
            var requestActionDTO = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
        }

        [Test, Category("Integration.terminalDropbox")]
        public async Task GetFileList_Run_ReturnsPayload()
        {
            //Arrange

            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();
            await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);

            var runUrl = GetTerminalRunUrl();
            var dataDTO = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());


            //Act
            var payloadDTOResult = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            // Assert
            Assert.NotNull(payloadDTOResult);
            Assert.NotNull(payloadDTOResult.CrateStorage);
            Assert.NotNull(payloadDTOResult.CrateStorage.Crates);
        }

        //var dataDTO = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();
        //AddOperationalStateCrate(dataDTO, new OperationalStateCM());

        //var payloadDTOResult = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        //var jsonData = ((JValue)(payloadDTOResult.CrateStorage.Crates[1].Contents)).Value.ToString();
        //var dropboxFileList = JsonConvert.DeserializeObject<List<string>>(jsonData);

        //Assert.NotNull(payloadDTOResult);
        //Assert.True(dropboxFileList.Any());
    }
}
