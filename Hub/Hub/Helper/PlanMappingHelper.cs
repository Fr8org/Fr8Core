using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Hub.Helper
{
    public static class PlanMappingHelper
    {
        // Manual mapping method to resolve DO-1164.
        public static PlanDTO MapPlanToDto(PlanDO curPlanDO)
        {
            var subPlanDTOList = curPlanDO.ChildNodes.OfType<SubplanDO>()
                .OrderBy(x => x.Ordering)
                .ToList()
                .Select((SubplanDO x) =>
                {
                    var pntDTO = Mapper.Map<FullSubplanDto>(x);

                    pntDTO.Activities = x.ChildNodes.OrderBy(y => y.Ordering).Select(Mapper.Map<ActivityDTO>).ToList();

                    return pntDTO;
                }).ToList();
            
            var result = new PlanDTO()
            {
                    Description = curPlanDO.Description,
                    Id = curPlanDO.Id,
                    Name = curPlanDO.Name,
                    PlanState = PlanState.IntToString(curPlanDO.PlanState),
                    Visibility = new PlanVisibilityDTO() { Hidden = curPlanDO.Visibility.BooleanValue() },
                    StartingSubPlanId = curPlanDO.StartingSubPlanId,
                    SubPlans = subPlanDTOList,
                    Fr8UserId = curPlanDO.Fr8AccountId,
                    Tag = curPlanDO.Tag,
                    Category = curPlanDO.Category,
                    LastUpdated = curPlanDO.LastUpdated
            };

            return result;
        }
    }
}