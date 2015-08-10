using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Interfaces
{
    public interface INegotiation
    {
        List<int> GetAnswerIDs(NegotiationDO curNegotiationDO);
        IList<int?> GetAnswerIDsByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow);

        void CreateQuasiEmailForBookingRequest(IUnitOfWork uow, NegotiationDO curNegotiationDO, UserDO curUserDO,
            Dictionary<QuestionDO, AnswerDO> currentAnswers);

        IEnumerable<string> GetSummaryText(NegotiationDO curNegotiationDO);
        void Resolve(int curNegotiationId);
        NegotiationDO Update(IUnitOfWork uow, NegotiationDO submittedNegotiationDO);
    }
}