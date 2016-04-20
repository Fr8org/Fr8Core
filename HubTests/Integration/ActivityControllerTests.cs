using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace HubTests.Integration
{
    [Explicit]
    [Category("HubTests.Integration")]
    public class ActivityController_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "Hub"; }
        }
        [Test]
        public async Task GetSolutionListByTerminalName_EndToEnd()
        {
            await GetDocuSignSolutionList();
            await GetFr8CoreSolutionList();
        }

        [Test]
        private async Task GetDocuSignSolutionList()
        {
            var baseUrl = GetHubApiBaseUrl();
            var getSolutionListUrl = baseUrl + "activities/Documentation";
            var emptyActivityDTO = new ActivityDTO { Documentation = "Terminal=terminalDocuSign" };
            var solutionNameList = await HttpPostAsync<ActivityDTO, List<string>>(getSolutionListUrl, emptyActivityDTO);
            Assert.IsNotNull(solutionNameList);
            Assert.IsTrue(solutionNameList.Any());
            Assert.Contains("Mail_Merge_Into_DocuSign", solutionNameList);
            Assert.Contains("Extract_Data_From_Envelopes", solutionNameList);
            Assert.Contains("Track_DocuSign_Recipients", solutionNameList);
            Assert.Contains("Generate_DocuSign_Report", solutionNameList);
            Assert.Contains("Archive_DocuSign_Template", solutionNameList);
        }
        [Test]
        private async Task GetFr8CoreSolutionList()
        {
            var baseUrl = GetHubApiBaseUrl();
            var getSolutionListUrl = baseUrl + "activities/Documentation";
            var emptyActivityDTO = new ActivityDTO { Documentation = "Terminal=terminalFr8Core" };
            var solutionNameList = await HttpPostAsync<ActivityDTO, List<string>>(getSolutionListUrl, emptyActivityDTO);
            Assert.IsNotNull(solutionNameList);
            Assert.IsTrue(solutionNameList.Any());
            Assert.Contains("FindObjects_Solution", solutionNameList);
            Assert.Contains("SearchFr8Warehouse", solutionNameList);
        }
    }
}
