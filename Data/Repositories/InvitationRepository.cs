using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class InvitationRepository : GenericRepository<InvitationDO>, IInvitationRepository
    {
        internal InvitationRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }

    public interface IInvitationRepository : IGenericRepository<InvitationDO>
    {

    }
}
