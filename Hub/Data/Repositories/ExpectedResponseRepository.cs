using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ExpectedResponseRepository : GenericRepository<ExpectedResponseDO>, IExpectedResponseRepository
    {
        internal ExpectedResponseRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IExpectedResponseRepository : IGenericRepository<ExpectedResponseDO>
    {

    }
}