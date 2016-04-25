using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.PlanDescriptions
{
    public class ActivityTransitionRepository : GenericRepository<ActivityTransitionDO>, IActivityTransitionRepository
    {
        public ActivityTransitionRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IActivityTransitionRepository : IGenericRepository<ActivityTransitionDO> { }
}
