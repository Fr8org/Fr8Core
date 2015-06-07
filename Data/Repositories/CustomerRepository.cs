using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class CustomerRepository : GenericRepository<CustomerDO>, ICustomerRepository
    {
        public CustomerRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface ICustomerRepository : IGenericRepository<CustomerDO>
    {

    }
}


