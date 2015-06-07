using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantICS.DDay.iCal.Interfaces;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;

namespace KwasantCore.Services
{
    public class InvitationResponse
    {
        public void Process(IUnitOfWork uow, InvitationResponseDO curInvitationResponse)
        {
            var icsAttachment = curInvitationResponse.Attachments
                .FirstOrDefault(a => string.Equals(a.Type, "text/calendar", StringComparison.Ordinal) ||
                                     string.Equals(a.Type, "application/ics", StringComparison.Ordinal));
            if (icsAttachment == null)
                throw new ArgumentException("Invitation response doesn't contain ics attachments.", "curInvitationResponse");
            iCalendarSerializer serializer = new iCalendarSerializer();
            var curCalendars = (IICalendarCollection)serializer.Deserialize(icsAttachment.GetData(), Encoding.UTF8);
            if (curCalendars.Count == 0)
                throw new ArgumentException("Invalid ics attachment. No calendars deserialized.", "curInvitationResponse");

            var curCalendar = curCalendars[0];
            if (!string.Equals(curCalendar.Method, "REPLY", StringComparison.Ordinal))
                throw new ArgumentException("Invalid ics attachment. METHOD property must be REPLY.", "curInvitationResponse");

            if (curCalendar.Events.Count == 0)
                throw new ArgumentException("Invalid ics attachment. No events deserialized.", "curInvitationResponse");

            var curIcsEvent = curCalendar.Events[0];
            var curIcsAttendee = curIcsEvent.Attendees
                .FirstOrDefault(a => string.Equals(
                    a.Value.OriginalString.Remove(0, "mailto:".Length),
                    curInvitationResponse.From.Address,
                    StringComparison.OrdinalIgnoreCase));
            if (curIcsAttendee == null)
                throw new ArgumentException("Invalid ics attachment. Cannot find sender's attendee.", "curInvitationResponse");

            var curEventGuid = curIcsEvent.UID;

            var curAttendee = uow.AttendeeRepository.GetQuery().FirstOrDefault(a => a.EmailAddressID == curInvitationResponse.From.Id && a.Event.ExternalGUID == curEventGuid);
            if (curAttendee == null)
                throw new EntityNotFoundException<AttendeeDO>(string.Format("Cannot find an attendee with EmailAddressID: {0} and Event.ExternalGUID: {1}.", 
                    curInvitationResponse.From.Id,
                    curEventGuid));

            curInvitationResponse.Attendee = curAttendee;
            curInvitationResponse.AttendeeId = curAttendee.Id;
            curAttendee.ParticipationStatus = ConvertStringParticipationStatus(curIcsAttendee.ParticipationStatus);
        }

        private int ConvertStringParticipationStatus(string participationStatus)
        {
            switch (participationStatus)
            {
                case KwasantICS.DDay.iCal.ParticipationStatus.NeedsAction:
                    return ParticipationStatus.NeedsAction;
                case KwasantICS.DDay.iCal.ParticipationStatus.Accepted:
                    return ParticipationStatus.Accepted;
                case KwasantICS.DDay.iCal.ParticipationStatus.Tentative:
                    return ParticipationStatus.Tentative;
                case KwasantICS.DDay.iCal.ParticipationStatus.Declined:
                    return ParticipationStatus.Declined;
                case KwasantICS.DDay.iCal.ParticipationStatus.Delegated:
                    return ParticipationStatus.Delegated;
                default:
                    throw new ArgumentOutOfRangeException("participationStatus", participationStatus, "Unknown participation status.");
            }
        }
    }
}
