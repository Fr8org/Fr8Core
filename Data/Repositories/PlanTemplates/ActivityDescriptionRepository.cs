using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.PlanDescriptions
{
    public class ActivityDescriptionRepository : GenericRepository<ActivityDescriptionDO>, IActivityDescriptionRepository
    {
        public ActivityDescriptionRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IActivityDescriptionRepository : IGenericRepository<ActivityDescriptionDO> { }
}
