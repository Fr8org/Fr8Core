
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
        public static RouteFullDTO MapRouteToDto(IUnitOfWork uow, RouteDO curRouteDO)
        {
            var subrouteDTOList = uow.SubrouteRepository
                .GetQuery()
                .Include(x => x.ChildNodes)
                .Where(x => x.ParentRouteNodeId == curRouteDO.Id)
                .OrderBy(x => x.Ordering)
                .ToList()
                .Select((SubrouteDO x) =>
                {
                    var pntDTO = Mapper.Map<FullSubrouteDTO>(x);

                    pntDTO.Actions = x.ChildNodes.OrderBy(y => y.Ordering).Select(Mapper.Map<ActionDTO>).ToList();

                    return pntDTO;
                }).ToList();

            RouteFullDTO result = new RouteFullDTO()
            {
                Description = curRouteDO.Description,
                Id = curRouteDO.Id,
                Name = curRouteDO.Name,
                RouteState = curRouteDO.RouteState,
                StartingSubrouteId = curRouteDO.StartingSubrouteId,
                Subroutes = subrouteDTOList,
                Fr8UserId = curRouteDO.Fr8Account.Id
            };

            return result;
        }
    }
}