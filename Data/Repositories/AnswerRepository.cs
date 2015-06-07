using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AnswerRepository : GenericRepository<AnswerDO>, IAnswerRepository
    {
        internal AnswerRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IAnswerRepository : IGenericRepository<AnswerDO>
    {

    }
}