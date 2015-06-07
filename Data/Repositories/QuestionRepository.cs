using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class QuestionRepository : GenericRepository<QuestionDO>, IQuestionRepository
    {
        internal QuestionRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IQuestionRepository : IGenericRepository<QuestionDO>
    {

    }
}
