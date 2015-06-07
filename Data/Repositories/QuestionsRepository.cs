using Data.Interfaces;
using Data.Entities;

namespace Data.Repositories
{
    public class QuestionsRepository : GenericRepository<QuestionDO>, IQuestionsRepository
    {
        internal QuestionsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

     public interface IQuestionsRepository : IGenericRepository<QuestionDO>
    {

    }
}
