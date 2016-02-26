using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Generate_DocuSign_Report_v1_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async Task Generate_DocuSign_Report_EndToEnd()
        {
            try
            {
                var plan = await CreateSolution();
                var solution = ExtractSolution(plan);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<RouteFullDTO> CreateSolution()
        {
            var solutionCreateUrl = _baseUrl + "activities/create?solutionName=Generate_DocuSign_Report";
            var plan = await HttpPostAsync<string, RouteFullDTO>(solutionCreateUrl, null);

            return plan;
        }

        private ActivityDTO ExtractSolution(RouteFullDTO plan)
        {
            var solution = plan.Subroutes
                .FirstOrDefault()
                .Activities
                .FirstOrDefault();

            return solution;
        }
    }
}
