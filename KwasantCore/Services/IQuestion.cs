using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Services
{
    public interface IQuestion
    {
        QuestionDO Update(IUnitOfWork uow, QuestionDO submittedQuestionDO);
    }
}