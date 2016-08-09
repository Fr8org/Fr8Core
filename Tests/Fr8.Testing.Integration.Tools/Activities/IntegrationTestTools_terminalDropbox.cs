using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Testing.Integration;
using NUnit.Framework;
using Fr8.Testing.Unit.Fixtures;

namespace Fr8.Testing.Integration.Tools.Activities
{
    public class IntegrationTestTools_terminalDropbox
    {
        private readonly BaseHubIntegrationTest _baseHubITest;
        private Terminals.IntegrationTestTools_terminalDropbox _terminalDropboxTestTools;

        public IntegrationTestTools_terminalDropbox(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
            _terminalDropboxTestTools = new Terminals.IntegrationTestTools_terminalDropbox(_baseHubITest);
        }

        public async Task<ActivityDTO> AddAndConfigure_GetFileList(PlanDTO plan, int ordering)
        {
            var dropboxGetFileListActivityDto = FixtureData.Get_File_List_v1_InitialConfiguration();

            var activityName = "Get_File_List";
            var activityCategoryParam = ActivityCategories.ReceiveId.ToString();
            var activityTemplates = await _baseHubITest
                .HttpGetAsync<List<WebServiceActivitySetDTO>>(
                _baseHubITest.GetHubApiBaseUrl() + "webservices?id=" + activityCategoryParam);
            var apmActivityTemplate = activityTemplates
                .SelectMany(a => a.Activities)
                .Select(x => new ActivityTemplateSummaryDTO
                {
                    Name = x.Name,
                    Version = x.Version,
                    TerminalName = x.Terminal.Name,
                    TerminalVersion = x.Terminal.Version
                })
                .Single(a => a.Name == activityName);

            dropboxGetFileListActivityDto.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.SubPlans.FirstOrDefault();
            dropboxGetFileListActivityDto.ParentPlanNodeId = subPlan.SubPlanId;
            dropboxGetFileListActivityDto.RootPlanNodeId = plan.Id;
            dropboxGetFileListActivityDto.Ordering = ordering;

            //call initial configuration to server
            dropboxGetFileListActivityDto = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseHubITest.GetHubApiBaseUrl() + "activities/save",
                dropboxGetFileListActivityDto);
            dropboxGetFileListActivityDto.AuthToken = FixtureData.GetDropboxAuthorizationToken();
            dropboxGetFileListActivityDto = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseHubITest.GetHubApiBaseUrl() + "activities/configure",
                dropboxGetFileListActivityDto);
            var initialcrateStorage = _baseHubITest.Crate.FromDto(dropboxGetFileListActivityDto.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaultDropboxAuthToken = stAuthCrate == null;

            Assert.AreEqual(true, defaultDropboxAuthToken, $"{activityName}: Dropbox require authentication. They might be a problem with default authentication tokens and KeyVault authorization mode");

            initialcrateStorage = _baseHubITest.Crate.FromDto(dropboxGetFileListActivityDto.CrateStorage);
            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), $"{activityName}: Crate StandardConfigurationControlsCM is missing in API response.");

            var controlsCrate = initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            //set the value of dropdown to file
            var controls = controlsCrate.Content.Controls;
            var folderControl = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "FileList");
            Assert.IsNotNull(folderControl, "Get_File_List: DropDownList control for FileList value selection was not found");
            folderControl.Value = "test file.txt";
            folderControl.selectedKey = "test file.txt";

            //call followup configuration
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(dropboxGetFileListActivityDto))
            {
                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Add(controlsCrate);
            }
            dropboxGetFileListActivityDto = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseHubITest.GetHubApiBaseUrl() + "activities/save", dropboxGetFileListActivityDto);
            dropboxGetFileListActivityDto = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseHubITest.GetHubApiBaseUrl() + "activities/configure", dropboxGetFileListActivityDto);

            return await Task.FromResult(dropboxGetFileListActivityDto);
        }
    }
}