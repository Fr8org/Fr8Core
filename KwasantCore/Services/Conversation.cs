using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace KwasantCore.Services
{
    public class Conversation
    {
        public static BookingRequestDO Match(IUnitOfWork uow, Dictionary<String, String> headers, IEnumerable<MailAddress> toAddresses, String subject, String fromAddress)
        {
            var matchedBookingRequest = MatchByHeaders(uow, headers);
            if (matchedBookingRequest == null)
                matchedBookingRequest = MatchByToAddress(uow, toAddresses);
            if (matchedBookingRequest == null)
                matchedBookingRequest = MatchBySubject(uow, subject, fromAddress);

            return matchedBookingRequest;
        }

        private static BookingRequestDO MatchByHeaders(IUnitOfWork uow, Dictionary<String, String> headers)
        {
            const string referencesKey = "References";
            if (headers == null)
                return null;

            if (headers.ContainsKey(referencesKey))
            {
                var referencesValue = headers[referencesKey];
                var eachReference = referencesValue.Split(' ', '\t');

                var potentialMatch = uow.EmailRepository.GetQuery().FirstOrDefault(e => eachReference.Contains(e.MessageID));

                if (potentialMatch == null)
                    return null;
                
                //Now we check if it's a conversation member, or the action booking request
                if (potentialMatch.ConversationId.HasValue)
                {
                    //It's a conversation member
                    return uow.BookingRequestRepository.GetByKey(potentialMatch.ConversationId);
                }
                return uow.BookingRequestRepository.GetByKey(potentialMatch.Id);
            }
            return null;
        }

        private static BookingRequestDO MatchByToAddress(IUnitOfWork uow, IEnumerable<MailAddress> toAddresses)
        {
            if (toAddresses == null)
                return null;
            const string regexStr = @"(.*)\+(?<messageid>[a-z0-9]+)@(.*)";
            var regex = new Regex(regexStr);
            var matches = toAddresses.Select(a => regex.Match(a.Address).Groups["messageid"].Value)
                .Where(r => !String.IsNullOrWhiteSpace(r))
                .Select(
                    address =>
                    {
                        int result;
                        if (int.TryParse(address, out result))
                            return result;
                        return -1;
                    }
                )
                .Where(r => r != -1);

            var potentialBookingRequestIDs = matches.ToList();
            var potentialMatch = uow.BookingRequestRepository.GetQuery().FirstOrDefault(e => potentialBookingRequestIDs.Contains(e.Id));
            return potentialMatch;
        }

        private static BookingRequestDO MatchBySubject(IUnitOfWork uow, String subject, String fromAddress)
        {
            var fromID = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress).Id;
            if (fromID == 0)
                return null;

            var currentFromEmailID = fromID;

            return uow.BookingRequestRepository.GetQuery().FirstOrDefault(br =>
                (
                    br.State != BookingRequestState.Invalid &&  //Don't match to invalid booking requests
                    br.State != BookingRequestState.Finished    //Don't match to old booking requests
                ) &&
                (
                    "RE: " + br.Subject == subject ||           //If the new message is 'RE: [oldSubject]"
                             br.Subject == subject              //If the new message is '[oldSubject]'
                ) &&
                (
                    //We want to check if they're a participant in either the booking request, OR the booking requests conversation
                    br.ConversationMembers.Union(new[] { br }).Any(
                        c =>
                            c.Recipients.Any(r => r.EmailAddressID == currentFromEmailID) ||    //Check if they're a recipient
                            c.FromID == currentFromEmailID                                      //Check if they're the sender
                        )
                    )
                );
        }

        public static EmailDO AddEmail(IUnitOfWork uow, MailMessage message, BookingRequestDO existingBookingRequest)
        {
            var curEmail = Email.ConvertMailMessageToEmail(uow.EmailRepository, message);
            curEmail.ConversationId = existingBookingRequest.Id;
            uow.UserRepository.GetOrCreateUser(curEmail.From);
            if (existingBookingRequest.State == BookingRequestState.AwaitingClient ||
                existingBookingRequest.State == BookingRequestState.Resolved)
            {
                var br = ObjectFactory.GetInstance<BookingRequest>();
                br.Reactivate(uow, existingBookingRequest);
            }

            uow.SaveChanges();

            Email.FixInlineImages(curEmail);
            uow.SaveChanges();
            
            // alerts
            AlertManager.ConversationMemberAdded(existingBookingRequest.Id);
            AlertManager.ConversationMatched(curEmail.Id, curEmail.Subject, existingBookingRequest.Id);

            return curEmail;
        }
    }
}
