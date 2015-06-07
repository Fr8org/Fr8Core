using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using Utilities;
using Encoding = System.Text.Encoding;

namespace KwasantCore.Services
{
    public class Invitation
    {
        private readonly IConfigRepository _configRepository;
        private readonly EmailAddress _emailAddress;

        public Invitation(IConfigRepository configRepository, EmailAddress emailAddress)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
            _emailAddress = emailAddress;
        }

        public InvitationDO Generate(IUnitOfWork uow, int curType, AttendeeDO curAttendee, EventDO curEvent, String extraBodyMessage)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (curAttendee == null)
                throw new ArgumentNullException("curAttendee");
            if (curEvent == null)
                throw new ArgumentNullException("curEvent");

            string replyToEmail = _configRepository.Get("replyToEmail");

            var emailAddressRepository = uow.EmailAddressRepository;
            if (curEvent.Attendees == null)
                curEvent.Attendees = new List<AttendeeDO>();

            InvitationDO curInvitation = new InvitationDO();
            curInvitation.ConfirmationStatus = ConfirmationStatus.Unnecessary;

            var toEmailAddress = emailAddressRepository.GetOrCreateEmailAddress(curAttendee.EmailAddress.Address);
            toEmailAddress.Name = curAttendee.Name;
            curInvitation.AddEmailRecipient(EmailParticipantType.To, toEmailAddress);

            //configure the sender information
            var from = _emailAddress.GetFromEmailAddress(uow, toEmailAddress, curEvent.CreatedBy);
            curInvitation.From = from;
            curInvitation.FromName = from.ToDisplayName();

            var replyToAddress = emailAddressRepository.GetOrCreateEmailAddress(replyToEmail);
            curInvitation.ReplyToAddress = replyToAddress.Address;
            curInvitation.ReplyToName = replyToAddress.Name;
            curInvitation.HTMLText = extraBodyMessage;
            curInvitation.PlainText = extraBodyMessage;

            var userID = uow.UserRepository.GetOrCreateUser(curAttendee.EmailAddress).Id;
            ConfigureForType(curType, uow, curInvitation, curEvent, userID);
            
            //prepare the outbound email
            curInvitation.EmailStatus = EmailState.Queued;
            if (curEvent.Emails == null)
                curEvent.Emails = new List<EmailDO>();

            var calendar = Event.GenerateICSCalendarStructure(curEvent);
            AttachCalendarToEmail(calendar, curInvitation);

            curEvent.Emails.Add(curInvitation);

            uow.InvitationRepository.Add(curInvitation);

            return curInvitation;
        }

        private static String GetAuthTokenForBaseURL(IUnitOfWork uow, string userID)
        {
            return uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(Server.ServerUrl, userID, "Invitation", new Dictionary<string, object>
                {
                    {"action", "Clicked on Invitation Header"}
                });
        }

        private void ConfigureForType(int type, IUnitOfWork uow, InvitationDO curInvitation, EventDO curEvent, String userID)
        {
            String subject;
            String templateName;

            var toUserDO = uow.UserRepository.GetByKey(userID);
            var guessedTimeZone = toUserDO.GetOrGuessTimeZone();

            DateTimeOffset recipientStartDate;
            DateTimeOffset recipientEndDate;
            
            String timezone;

            if (guessedTimeZone != null)
            {
                timezone = guessedTimeZone.DisplayName;
                recipientStartDate = curEvent.StartDate.ToOffset(guessedTimeZone.GetUtcOffset(DateTime.Now));
                recipientEndDate = curEvent.EndDate.ToOffset(guessedTimeZone.GetUtcOffset(DateTime.Now));
            }
            else
            {
                recipientStartDate = curEvent.StartDate;
                recipientEndDate = curEvent.EndDate;
                timezone = "UTC" + (curEvent.StartDate.Offset.Ticks < 0 ? curEvent.StartDate.Offset.ToString() : "+" + curEvent.StartDate.Offset);
            }
            string endtime = recipientEndDate.ToString("hh:mm tt");

            string subjectDate = recipientStartDate.ToString("ddd MMM dd, yyyy hh:mm tt - ") + endtime;
            
            if (type == InvitationType.InitialInvite)
            {
                subject = String.Format(_configRepository.Get("emailSubject"), GetOriginatorName(curEvent), curEvent.Summary, subjectDate);
                curInvitation.InvitationType = InvitationType.InitialInvite;
                templateName = _configRepository.Get("InvitationInitial_template");
            }
            else
            {
                subject = String.Format(_configRepository.Get("emailSubjectUpdated"), GetOriginatorName(curEvent), curEvent.Summary, subjectDate);
                curInvitation.InvitationType = InvitationType.ChangeNotification;
                templateName = _configRepository.Get("InvitationUpdate_template");
            }

            const string whoWrapper = @"
<table cellpadding=""0"" cellspacing=""0"">
{0}
</table>
";

            const string whoFormat = @"
<tr>
    <td style=""color: rgb(34, 34, 34); font-family: Arial, sans-serif; font-size: 13px; padding-right: 10px;"">
        <div>
            <div style=""margin: 0px 0px 0.3em""><a href=""mailto:{0}"">{1}</a></div>
        </div>
    </td>
</tr>";
            var whoList = String.Join(Environment.NewLine, curEvent.Attendees.Select(a => String.Format(whoFormat, a.EmailAddress.Address, String.IsNullOrEmpty(a.Name) ? a.EmailAddress.Address : a.Name)));

            var whoListPlainText = String.Join(Environment.NewLine, curEvent.Attendees.Select(a => a.Name + " - " + a.EmailAddress.Address));

            curInvitation.Subject = subject;

            curInvitation.TagEmailToBookingRequest(curEvent.BookingRequest, false);

            uow.EnvelopeRepository.ConfigureTemplatedEmail(
                curInvitation, templateName, new Dictionary<string, object>
                {
                    {"description", curEvent.Description},
                    {
                        "time", curEvent.IsAllDay
                            ? "All day - " + recipientStartDate.ToString("ddd d MMM")
                            : recipientStartDate.ToString("ddd MMM d, yyyy hh:mm tt") + " - " +
                              recipientEndDate.ToString("hh:mm tt")
                    },
                    {"location", curEvent.Location},
                    {"wholist", String.Format(whoWrapper, whoList)},
                    {"linkto", GetAuthTokenForBaseURL(uow, userID)},
                    {"summary", curEvent.Summary},
                    {"timezone", timezone},
                    {"wholistplaintext", whoListPlainText},
                }
                );
        }

        private void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = GetAttachment(fileToAttach);

            attachmentDO.Email = emailDO;
            emailDO.Attachments.Add(attachmentDO);
        }


        private AttachmentDO GetAttachment(string fileToAttach)
        {
            return Email.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/ics", Name = "invite.ics" }
                    ) { TransferEncoding = TransferEncoding.Base64 });
        }

        //if we have a first name and last name, use them together
        //else if we have a first name only, use that
        //else if we have just an email address, use the portion preceding the @ unless there's a name
        //else throw
        public string GetOriginatorName(EventDO curEventDO)
        {
            UserDO originator = curEventDO.CreatedBy;
            return User.GetDisplayName(originator);
        }
    }
}
