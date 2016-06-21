using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using NUnit.Framework;
using Fr8.Testing.Integration;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Mail_Merge_Into_DocuSign_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
        }

        private void AddHubActivityTemplate(Fr8DataDTO dataDTO)
        {

            AddActivityTemplate(
              dataDTO,
              new ActivityTemplateDTO()
              {
                  Id = Guid.NewGuid(),
                  Name = "Load Excel File",
                  Label = "Load Excel File",
                  Tags = "Table Data Generator"
              }
          );

            AddActivityTemplate(
                dataDTO,
                new ActivityTemplateDTO()
                {
                    Id = Guid.NewGuid(),
                    Name = "Extract Spreadsheet Data",
                    Label = "Extract Spreadsheet Data",
                    Tags = "Table Data Generator"
                }
            );

            AddActivityTemplate(
               dataDTO,
               new ActivityTemplateDTO()
               {
                   Id = Guid.NewGuid(),
                   Name = "Get Google Sheet Data",
                   Label = "Get Google Sheet Data",
                   Tags = "Table Data Generator",
                   Category = ActivityCategory.Receivers
               }
           );
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            // Assert that DataSource ,  DocuSignTemplate and Button control are present
            Assert.AreEqual(3, controls.Controls.Count());
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "DataSource"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "DocuSignTemplate"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Type == "Button"));

            // Assert that DataSource dropdown contains sources and it should be only "Get"
            var dataSourceDropdown = (DropDownList)controls.Controls[0];
            Assert.AreEqual(3, dataSourceDropdown.ListItems.Count());

            // Assert that Dropdownlist  source is null.
            var templateDropdown = (DropDownList)controls.Controls[1];
            Assert.AreEqual(null, templateDropdown.Source);
        }

        private async Task<ActivityDTO> GetActivityDTO_WithDataStorage(string childAction)
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            AddHubActivityTemplate(requestDataDTO);
            
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            responseActionDTO.AuthToken = requestDataDTO.ActivityDTO.AuthToken;

            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                var dataSourceDropdown = (DropDownList)controls.Controls[0];
                dataSourceDropdown.Value = childAction;

                var availableTemplatesCM = crateStorage
                  .CrateContentsOfType<KeyValueListCM>(x => x.Label == "Available Templates")
                  .Single();
                Assert.IsTrue(availableTemplatesCM.Values.Count > 0);

                var templateDropdown = (DropDownList)controls.Controls[1];
                templateDropdown.Value = availableTemplatesCM.Values[0].Value;

                var continueButton = (Button)controls.Controls[2];
                continueButton.Clicked = true;
            }

            return responseActionDTO;
        }

        [Test]
        public async Task Mail_Merge_Into_DocuSign_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            AddHubActivityTemplate(requestDataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        [Test]
        public void Mail_Merge_Into_DocuSign_FollowUp_Configuration_Check_ChildActivity_Load_Excel_File()
        {
            //string childAction = "Load Excel File";
            //var configureUrl = GetTerminalConfigureUrl();

            //var requestActionDTO = HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_ActionDTO();

            //var responseActionDTO = await GetActionDTO_WithDataStorage(childAction);
            // responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
            //        configureUrl,
            //        responseActionDTO
            //    );
          
            //Assert.NotNull(responseActionDTO);
            //Assert.NotNull(responseActionDTO.CrateStorage);
            //Assert.AreEqual(1, responseActionDTO.ChildrenActions.Length);

            //// Assert that Selected child Action is present
            //Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x=> x.Label == "Load Excel File"));
        }

        [Test]
        public void Mail_Merge_Into_DocuSign_FollowUp_Configuration_Check_ChildActivity_Get_Google_Sheet_Data()
        {
            //string childAction = "Extract Spreadsheet Data";
            //var configureUrl = GetTerminalConfigureUrl();

            //var requestActionDTO = HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_ActionDTO();

            //var responseActionDTO = await GetActionDTO_WithDataStorage(childAction);
            //responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
            //       configureUrl,
            //       responseActionDTO
            //   );

            //Assert.NotNull(responseActionDTO);
            //Assert.NotNull(responseActionDTO.CrateStorage);
            //Assert.AreEqual(1, responseActionDTO.ChildrenActions.Length);

            //// Assert that Selected child Action is present
            //Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Extract Spreadsheet Data"));
        }

        [Test]
        public async Task Mail_Merge_Into_DocuSign_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

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
        public async Task Mail_Merge_Into_DocuSign_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

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
