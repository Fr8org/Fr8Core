using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class AttendeeRepository : GenericRepository<AttendeeDO>, IAttendeeRepository
    {
        internal AttendeeRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IAttendeeRepository : IGenericRepository<AttendeeDO>
    {

    }
}
