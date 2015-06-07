using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class InvitationResponseRepository : GenericRepository<InvitationResponseDO>, IInvitationResponseRepository
    {
        internal InvitationResponseRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface IInvitationResponseRepository : IGenericRepository<InvitationResponseDO>
    {

    }
}
