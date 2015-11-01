using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface IAnswer
    {
        AnswerDO Update(IUnitOfWork uow, AnswerDO submittedAnswerDO);
    }
}