using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface INegotiation
    {
        List<int> GetAnswerIDs(NegotiationDO curNegotiationDO);
        IList<int?> GetAnswerIDsByUser(NegotiationDO curNegotiationDO, DockyardAccountDO curDockyardAccountDO, IUnitOfWork uow);

        void CreateQuasiEmailForBookingRequest(IUnitOfWork uow, NegotiationDO curNegotiationDO, DockyardAccountDO curDockyardAccountDO,
            Dictionary<QuestionDO, AnswerDO> currentAnswers);

        IEnumerable<string> GetSummaryText(NegotiationDO curNegotiationDO);
        void Resolve(int curNegotiationId);
        NegotiationDO Update(IUnitOfWork uow, NegotiationDO submittedNegotiationDO);
    }
}