using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalAtlassianTests.Fixtures;

namespace terminalAtlassianTests.Integration
{
    [Explicit]
    public class Monitor_Jira_Changes_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalAtlassian";

        private const string IssueKey = "Issue Key";
        private const string ProjectName = "Project Name";
        private const string IssueResolution = "Issue Resolution";
        private const string IssuePriority = "Issue Priority";
        private const string IssueAssignee = "Issue Assignee Name";
        private const string IssueSummary = "Issue Summary";
        private const string IssueStatus = "Issue Status";
        private const string IssueDescription = "Issue Description";
        private const string EventType = "Event Type";

        private void AssertConfigureControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(2, control.Controls.Count, "Control count is not 2");
            Assert.IsTrue(control.Controls[0] is TextBlock, "First control isn't a TextBlock");
            Assert.IsTrue(control.Controls[1] is DropDownList, "Second control isn't a DropDownList");
            Assert.AreEqual("Description", control.Controls[0].Label, "Invalid Label on control");
            Assert.AreEqual("Description", control.Controls[0].Name, "Invalid Name on control");
        }

        private void AssertConfigureCrate(ICrateStorage crateStorage)
        {
            Assert.AreEqual(2, crateStorage.Count, "Crate storage count is not equal to 2");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "StandardConfigurationControlsCM count is not 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "FieldDescriptionsCM count is not 1");
            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
            var fieldDescriptions = crateStorage.CratesOfType<CrateDescriptionCM>().Single();
            Assert.AreEqual("Runtime Available Crates", fieldDescriptions.Label, "Monitor Atlassian Runtime Fields labeled FieldDescriptionsCM was not found");
            Assert.AreEqual(2, fieldDescriptions.Content.CrateDescriptions.Count(), "CrateDescriptions count is not 2");
            var crateDescription = fieldDescriptions.Content.CrateDescriptions.Where(t => t.ManifestType.Equals("Standard Payload Data"));
            var fields = crateDescription.Single().Fields;
            Assert.AreEqual("Monitor Atlassian Runtime Fields", crateDescription.Single().Label, "Monitor Atlassian Runtime Fields labeled CrateDescription was not found");
            Assert.AreEqual(9, crateDescription.Single().Fields.Count, "Published runtime field count is not 9");


            Assert.IsTrue(fields.Exists(x => x.Name == IssueKey), "IssueKey is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == ProjectName), "ProjectName is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == IssueResolution), "IssueResolution is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == IssuePriority), "IssuePriority is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == IssueAssignee), "IssueAssignee is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == IssueSummary), "IssueSummary is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == IssueStatus), "IssueStatus is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == IssueDescription), "IssueDescription is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == EventType), "EventType is not signalled");
        }

        private async Task<ActivityDTO> CompleteInitialConfiguration()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = HealthMonitor_FixtureData.Monitor_Jira_Events_v1_InitialConfiguration_Fr8DataDTO();
            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
        }

        private void AssertInitialConfigurationResponse(ActivityDTO responseDTO)
        {
            Assert.NotNull(responseDTO, "Response is null on initial configuration");
            Assert.NotNull(responseDTO.CrateStorage, "Crate storage is null on initial configuration");
            var crateStorage = Crate.FromDto(responseDTO.CrateStorage);
            AssertConfigureCrate(crateStorage);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task MonitorJiraEvents_Initial_Configuration_Check_Crate_Structure()
        {
            var responseDTO = await CompleteInitialConfiguration();
            AssertInitialConfigurationResponse(responseDTO);
        }
    }
}