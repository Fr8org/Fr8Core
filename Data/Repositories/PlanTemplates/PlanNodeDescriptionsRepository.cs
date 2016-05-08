using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.PlanDescriptions
{
    public class PlanNodeDescriptionsRepository : GenericRepository<PlanNodeDescriptionDO>, IPlanNodeDescriptionsRepository
    {
        public PlanNodeDescriptionsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IPlanNodeDescriptionsRepository : IGenericRepository<PlanNodeDescriptionDO> { }
}
