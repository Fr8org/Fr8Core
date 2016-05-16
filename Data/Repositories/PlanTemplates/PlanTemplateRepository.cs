using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.PlanDescriptions
{
    public class PlanTemplateRepository : GenericRepository<PlanTemplateDO>, IPlanTemplateRepository
    {
        public PlanTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    

    public interface IPlanTemplateRepository : IGenericRepository<PlanTemplateDO> { }
}
