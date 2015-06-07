using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Interfaces
{
    public interface IAttendee
    {
        AttendeeDO Create(IUnitOfWork uow, string emailAddressString, EventDO curEventDO, String name = null);
        List<AttendeeDO> ConvertFromString(IUnitOfWork uow, string curAttendees);
        void ManageNegotiationAttendeeList(IUnitOfWork uow, NegotiationDO negotiationDO, List<String> attendees);
        List<AttendeeDO> ManageAttendeeList(IUnitOfWork uow, IList<AttendeeDO> existingAttendeeSet, List<String> attendees);
        IList<Int32?> GetRespondedAnswers(IUnitOfWork uow, List<Int32> answerIDs, string userID);
        AnswerDO GetSelectedAnswer(QuestionDO curQuestion, IEnumerable<Int32?> curUserAnswers);
    }
}