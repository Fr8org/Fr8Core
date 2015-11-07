using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using KwasantWeb.ViewModels;

namespace KwasantCore.Interfaces
{
    public interface INegotiation
    {
        List<int> GetAnswerIDs(NegotiationDO curNegotiationDO);
        IList<int?> GetAnswerIDsByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow);

        void CreateQuasiEmailForBookingRequest(IUnitOfWork uow, NegotiationDO curNegotiationDO, UserDO curUserDO,
            Dictionary<QuestionDO, AnswerDO> currentAnswers);

        void Resolve(int curNegotiationId);
        NegotiationDO GetOrCreate(int? curNegotiationID, IUnitOfWork uow);
        void Update(IUnitOfWork uow, NegotiationVM submittedNegotiation, NegotiationDO curNegotiationDO);
    }
}