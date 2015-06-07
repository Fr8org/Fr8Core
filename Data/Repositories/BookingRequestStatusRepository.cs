using Data.Interfaces;
using Data.States.Templates;

namespace Data.Repositories
{
    public class BookingRequestStatusRepository : GenericRepository<_BookingRequestStateTemplate>, IBookingRequestDOStatusRepository
    {
        public BookingRequestStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IBookingRequestDOStatusRepository : IGenericRepository<_BookingRequestStateTemplate>
    {

    }
}


