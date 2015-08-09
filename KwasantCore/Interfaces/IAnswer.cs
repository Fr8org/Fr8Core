using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Interfaces
{
    public interface IAnswer
    {
        AnswerDO Update(IUnitOfWork uow, AnswerDO submittedAnswerDO);
    }
}