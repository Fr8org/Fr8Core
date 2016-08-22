using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using NUnit.Framework;
using Fr8.Testing.Unit.Fixtures;
using Data.States;

namespace Fr8.Testing.Integration.Tools.Plans
{
    public class IntegrationTestTools
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        public async Task<PlanDTO> CreateNewPlan(string planName = "")
        {
            var newPlan = FixtureData.CreateTestPlanDTO(planName);

            var planDTO = await _baseHubITest.HttpPostAsync<PlanNoChildrenDTO, PlanDTO>(_baseHubITest.GetHubApiBaseUrl() + "plans", newPlan);

            Assert.AreNotEqual(planDTO.Id, Guid.Empty, "New created Plan doesn't have a valid Id. Plan failed to be crated.");
            Assert.True(planDTO.SubPlans.Any(), "New created Plan doesn't have an existing sub plan.");

            return await Task.FromResult(planDTO);
        }

        public async Task<ContainerDTO> RunPlan(Guid planId)
        {
            var executionContainer = await _baseHubITest.HttpPostAsync<string, ContainerDTO>(_baseHubITest.GetHubApiBaseUrl() + "plans/run?planId=" + planId, null);

            Assert.IsNotNull(executionContainer, "Execution of plan failed. ContainerDTO is missing as a response");
            if (executionContainer.CurrentPlanType != Infrastructure.Data.Constants.PlanType.Monitoring)
                Assert.AreEqual(executionContainer.State, State.Completed, "Execution of plan failed. Container state is not completed");

            return executionContainer;
        }
    }
}
