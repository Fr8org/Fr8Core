using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class QuestionResponseRepository : GenericRepository<QuestionResponseDO>, IQuestionResponseRepository
    {
        internal QuestionResponseRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IQuestionResponseRepository : IGenericRepository<QuestionResponseDO>
    {

    }
}
