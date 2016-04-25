using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.PlanDescriptions
{
    public class PlanDescriptionsRepository : GenericRepository<PlanDescriptionDO>, IPlanDescriptionsRepository
    {
        public PlanDescriptionsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IPlanDescriptionsRepository : IGenericRepository<PlanDescriptionDO> { }
}
