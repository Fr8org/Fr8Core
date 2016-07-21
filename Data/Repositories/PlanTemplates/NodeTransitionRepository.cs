using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.PlanDescriptions
{
    public class NodeTransitionRepository : GenericRepository<NodeTransitionDO>, INodeTransitionRepository
    {
        public NodeTransitionRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface INodeTransitionRepository : IGenericRepository<NodeTransitionDO> { }
}
