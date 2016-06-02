using System;
using System.Threading.Tasks;
using AutoMapper;
using Fr8Data.DataTransferObjects;
using TerminalBase.Infrastructure;

namespace TerminalBase.Helpers
{
    public class PlanHelper
    {
        private readonly IHubCommunicator _hubCommunicator;

        public PlanHelper(IHubCommunicator hubCommunicator)
        {
            _hubCommunicator = hubCommunicator;
        }

        protected async Task<PlanDTO> GetPlansByActivity(string activityId)
        {
            return await _hubCommunicator.GetPlansByActivity(activityId);
        }

        protected async Task<PlanDTO> UpdatePlan(PlanNoChildrenDTO plan)
        {
            return await _hubCommunicator.UpdatePlan(plan);
        }

        /// <summary>
        /// Update Plan name if the current Plan name is the same as the passed parameter OriginalPlanName to avoid overwriting the changes made by the user
        /// </summary>
        /// <param name="terminalActivity"></param>
        /// <param name="OriginalPlanName"></param>
        /// <returns></returns>
        public async Task<PlanFullDTO> UpdatePlanName(Guid activityId, string OriginalPlanName, string NewPlanName)
        {
            try
            {
                PlanDTO plan = await GetPlansByActivity(activityId.ToString());
                if (plan != null && plan.Plan.Name.Equals(OriginalPlanName, StringComparison.OrdinalIgnoreCase))
                {
                    plan.Plan.Name = NewPlanName;

                    var emptyPlanDTO = Mapper.Map<PlanNoChildrenDTO>(plan.Plan);
                    plan = await UpdatePlan(emptyPlanDTO);
                }

                return plan.Plan;

            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public async Task<PlanFullDTO> UpdatePlanCategory(Guid activityId, string category)
        {
            PlanDTO plan = await GetPlansByActivity(activityId.ToString());
            if (plan != null && plan.Plan != null)
            {
                plan.Plan.Category = category;

                var emptyPlanDTO = Mapper.Map<PlanNoChildrenDTO>(plan.Plan);
                plan = await UpdatePlan(emptyPlanDTO);
            }

            return plan.Plan;
        }
    }
}
