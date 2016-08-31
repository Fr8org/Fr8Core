using System;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    public class PlanService
    {
        private readonly IHubCommunicator _hubCommunicator;

        public PlanService(IHubCommunicator hubCommunicator)
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
        public async Task<PlanDTO> UpdatePlanName(Guid activityId, string OriginalPlanName, string NewPlanName)
        {
            try
            {
                PlanDTO plan = await GetPlansByActivity(activityId.ToString());
                if (plan != null && plan.Name.Equals(OriginalPlanName, StringComparison.OrdinalIgnoreCase))
                {
                    plan.Name = NewPlanName;

                    var emptyPlanDTO = Mapper.Map<PlanNoChildrenDTO>(plan);
                    plan = await UpdatePlan(emptyPlanDTO);
                }

                return plan;

            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public async Task<PlanDTO> UpdatePlanCategory(Guid activityId, string category)
        {
            PlanDTO plan = await GetPlansByActivity(activityId.ToString());
            if (plan != null)
            {
                plan.Category = category;

                var emptyPlanDTO = Mapper.Map<PlanNoChildrenDTO>(plan);
                plan = await UpdatePlan(emptyPlanDTO);
            }

            return plan;
        }

        public async Task<PlanDTO> ConfigureAsApp(Guid activityId, string launchUrl, string name)
        {
            PlanDTO plan = await GetPlansByActivity(activityId.ToString());
            if (plan != null)
            {
                plan.IsApp = true;
                plan.AppLaunchUrl = launchUrl;
                plan.Name = name;

                var emptyPlanDTO = Mapper.Map<PlanNoChildrenDTO>(plan);
                plan = await UpdatePlan(emptyPlanDTO);
            }

            return plan;
        }
    }
}
