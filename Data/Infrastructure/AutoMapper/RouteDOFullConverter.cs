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
        : ITypeConverter<PlanDO, RouteFullDTO>
    {
        public const string UnitOfWork_OptionsKey = "UnitOfWork";


        public RouteFullDTO Convert(ResolutionContext context)
        {
            var plan = (PlanDO)context.SourceValue;
            var uow = (IUnitOfWork)context.Options.Items[UnitOfWork_OptionsKey];

            if (plan == null)
            {
                return null;
            }

            var subrouteDTOList = uow.SubrouteRepository
                .GetQuery()
                .Include(x => x.ChildNodes)
                .Where(x => x.ParentRouteNodeId == plan.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select((SubrouteDO x) =>
                {
                    var pntDTO = Mapper.Map<FullSubrouteDTO>(x);
                    pntDTO.Actions = x.ChildNodes.OfType<ActionDO>().Select(Mapper.Map<ActionDTO>).ToList();
                    return pntDTO;
                }).ToList();

            var result = Mapper.Map<RouteFullDTO>(Mapper.Map<RouteEmptyDTO>(plan));
            result.Subroutes = subrouteDTOList;
            result.Fr8UserId = plan.Fr8Account.Id;

            return result;
        }
    }
}
