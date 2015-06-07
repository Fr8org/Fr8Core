using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestRepository : GenericRepository<BookingRequestDO>, IBookingRequestDORepository
    {
        internal BookingRequestRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IBookingRequestDORepository : IGenericRepository<BookingRequestDO>
    {

    }
}
