
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace HubWeb.Controllers.Helpers
{
    public static class RouteMappingHelper
    {
        // Manual mapping method to resolve DO-1164.
        public static RouteFullDTO MapRouteToDto(IUnitOfWork uow, PlanDO curPlanDO)
        {
            var subrouteDTOList = curPlanDO.ChildNodes.OfType<SubrouteDO>()
                .OrderBy(x => x.Ordering)
                .ToList()
                .Select((SubrouteDO x) =>
                {
                    var pntDTO = Mapper.Map<FullSubrouteDTO>(x);

                    pntDTO.Activities = x.ChildNodes.OrderBy(y => y.Ordering).Select(Mapper.Map<ActivityDTO>).ToList();

                    return pntDTO;
                }).ToList();

            var result = new RouteFullDTO()
            {
                Description = curPlanDO.Description,
                Id = curPlanDO.Id,
                Name = curPlanDO.Name,
                RouteState = curPlanDO.RouteState,
                StartingSubrouteId = curPlanDO.StartingSubrouteId,
                Subroutes = subrouteDTOList,
                Fr8UserId = curPlanDO.Fr8AccountId,
                Tag = curPlanDO.Tag
            };

            return result;
        }
    }
}