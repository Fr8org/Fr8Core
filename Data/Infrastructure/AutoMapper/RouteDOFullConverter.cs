using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Data.Infrastructure.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert RouteDO to FullRouteDTO.
    /// </summary>
    public class RouteDOFullConverter
        : ITypeConverter<RouteDO, RouteFullDTO>
    {
        public const string UnitOfWork_OptionsKey = "UnitOfWork";


        public RouteFullDTO Convert(ResolutionContext context)
        {
            var route = (RouteDO)context.SourceValue;
            var uow = (IUnitOfWork)context.Options.Items[UnitOfWork_OptionsKey];

            if (route == null)
            {
                return null;
            }

            var subrouteDTOList = uow.SubrouteRepository
                .GetQuery()
                .Include(x => x.ChildNodes)
                .Where(x => x.ParentRouteNodeId == route.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select((SubrouteDO x) =>
                {
                    var pntDTO = Mapper.Map<FullSubrouteDTO>(x);
                    pntDTO.Activities = x.ChildNodes.OfType<ActivityDO>().Select(Mapper.Map<ActivityDTO>).ToList();
                    return pntDTO;
                }).ToList();

            var result = Mapper.Map<RouteFullDTO>(Mapper.Map<RouteEmptyDTO>(route));
            result.Subroutes = subrouteDTOList;
            result.Fr8UserId = route.Fr8Account.Id;

            return result;
        }
    }
}
