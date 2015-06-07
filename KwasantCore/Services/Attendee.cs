using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;


namespace KwasantCore.Services
{
    public class Attendee : IAttendee
    {
        private readonly EmailAddress _emailAddress;

        public Attendee(EmailAddress emailAddress)
        {
            _emailAddress = emailAddress;
        }

        public AttendeeDO Create(IUnitOfWork uow, string emailAddressString, EventDO curEventDO, String name = null)
        {
            //create a new AttendeeDO
            //get or create the email address and associate it.
            AttendeeDO curAttendee = new AttendeeDO();
               
            var emailAddressRepository = uow.EmailAddressRepository;
            EmailAddressDO emailAddress = emailAddressRepository.GetOrCreateEmailAddress(emailAddressString, name);
            curAttendee.EmailAddressID = emailAddress.Id;
            curAttendee.EmailAddress = emailAddress;
            curAttendee.ParticipationStatus = ParticipationStatus.NeedsAction;
            curAttendee.Name = emailAddress.Name;
            curAttendee.Event = curEventDO;  //do we have to also manually set the EventId? Seems unDRY
            //uow.AttendeeRepository.Add(curAttendee); //is this line necessary?
            
            return curAttendee;
        }

        public List<AttendeeDO> ConvertFromString(IUnitOfWork uow, string curAttendees)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (String.IsNullOrEmpty(curAttendees))
                return new List<AttendeeDO>();

            var attendees = curAttendees.Split(',');
            return ConvertFromStringList(uow, attendees);
        }

        private List<AttendeeDO> ConvertFromStringList(IUnitOfWork uow, IList<String> curAttendees)
        {
            if (curAttendees == null)
                return new List<AttendeeDO>();


            return curAttendees
                .Select(attendee => new AttendeeDO
                                        {
                                            EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee, attendee),
                                            Name = attendee
                                        })
                .ToList();
        }

        public void ManageNegotiationAttendeeList(IUnitOfWork uow, NegotiationDO negotiationDO, List<String> attendees)
        {
            var existingAttendeeSet = negotiationDO.Attendees ?? new List<AttendeeDO>();
            
            List<AttendeeDO> newAttendees = ManageAttendeeList(uow, existingAttendeeSet, attendees);

            foreach (var attendee in newAttendees)
            {
                attendee.Negotiation = negotiationDO;
                attendee.NegotiationID = negotiationDO.Id;
                uow.AttendeeRepository.Add(attendee);
            }
        }

        public List<AttendeeDO> ManageAttendeeList(IUnitOfWork uow, IList<AttendeeDO> existingAttendeeSet, List<String> attendees)
        {
            var attendeesToDelete = existingAttendeeSet.Where(attendee => !attendees.Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            var newAttendees = attendees
                .Where(att => !existingAttendeeSet.Select(a => a.EmailAddress.Address).Contains(att))
                .ToList();
            return ConvertFromStringList(uow, newAttendees);
        }

        public IList<Int32?> GetRespondedAnswers(IUnitOfWork uow, List<Int32> answerIDs, string userID )
        {
            //Query all of the actual responses....
            return  uow.QuestionResponseRepository.GetQuery()
                //...find all of this attendee's responses that are associated with this Negotiation's Answers
                .Where(qr => qr.AnswerID.HasValue && answerIDs.Contains(qr.AnswerID.Value) && qr.UserID == userID)
                //...and create a list of just the Answers for which there is a response from this attendee
                .Select(a => a.AnswerID).ToList();
        }

        public AnswerDO GetSelectedAnswer(QuestionDO curQuestion, IEnumerable<Int32?> curUserAnswers)
        {
            //select the Answer that is in our list of the answers to which the  attendee has responded

            return curQuestion.Answers.FirstOrDefault(a => curUserAnswers.Contains(a.Id));
        }

       
       

       
    }
}
