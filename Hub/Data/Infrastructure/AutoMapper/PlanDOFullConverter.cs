using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Data.Infrastructure.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert PlanDO to FullPlanDTO.
    /// </summary>
    public class PlanDOFullConverter
        : ITypeConverter<PlanDO, PlanDTO>
    {
        public const string UnitOfWork_OptionsKey = "UnitOfWork";


        public PlanDTO Convert(ResolutionContext context)
        {
            var plan = (PlanDO)context.SourceValue;
            var uow = (IUnitOfWork)context.Options.Items[UnitOfWork_OptionsKey];

            if (plan == null)
            {
                return null;
            }

            var subPlanDTOList = uow.PlanRepository.GetById<PlanDO>(plan.Id).ChildNodes.OfType<SubplanDO>()
                .Select(x =>
                {
                    var pntDTO = Mapper.Map<FullSubplanDto>(x);
                    pntDTO.Activities = x.ChildNodes.OfType<ActivityDO>().Select(Mapper.Map<ActivityDTO>).ToList();
                    return pntDTO;
                }).ToList();

            var result = Mapper.Map<PlanDTO>(Mapper.Map<PlanNoChildrenDTO>(plan));
            result.SubPlans = subPlanDTOList;
            result.Fr8UserId = plan.Fr8Account.Id;

            return result;
        }
    }
}
