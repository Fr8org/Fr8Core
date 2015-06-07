using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Segment;
using StructureMap;
using Utilities;
using Logger = Utilities.Logging.Logger;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Booker")]
    public class EventController : Controller
    {
        public const string DateStandardFormat = @"yyyy-MM-ddTHH\:mm\:ss\z";

        private readonly Event _event;
        private readonly Attendee _attendee;
        private readonly IMappingEngine _mappingEngine;

        public EventController() 
        {
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>(); // TODO: inject dependency via a constructor parameter
            _event = ObjectFactory.GetInstance<Event>();
            _attendee = ObjectFactory.GetInstance<Attendee>();
        }

        public ActionResult New(int bookingRequestID, int calendarID, string start, string end)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var createdEvent = CreateNewEvent(uow, bookingRequestID, calendarID, start, end);
                _event.Create(createdEvent, uow);

                uow.EventRepository.Add(createdEvent);
                uow.SaveChanges();

                //put it in a view model to hand to the view
                var curEventVM = _mappingEngine.Map<EventDO, EventVM>(createdEvent);
                curEventVM.IsNew = true;
                //construct a Calendar view model for this Calendar View 
                return View("~/Views/Event/Edit.cshtml", curEventVM);
            }
        }

        [HttpPost]
        public ActionResult NewTimeSlot(int calendarID, string start, string end, bool mergeEvents = false, string eventDescription = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var createdEvent = CreateNewEvent(uow, null, calendarID, start, end);

                createdEvent.Description = createdEvent.Summary = eventDescription;
                createdEvent.CreatedByID = uow.CalendarRepository.GetByKey(calendarID).OwnerID;
                createdEvent.EventStatus = EventState.ProposedTimeSlot;
                
                uow.EventRepository.Add(createdEvent);
                //And now we merge changes
                if (mergeEvents)
                    MergeTimeSlots(uow, createdEvent);

                uow.SaveChanges();

                return Json(true);
            }
        }

        private void MergeTimeSlots(IUnitOfWork uow, EventDO updatedEvent)
        {
            //We want to merge existing events so we get continous blocks whenever they're 'filled in'.

            //We want to merge in the following situtations:

            //We have an existing event window ending at the same time as our new window starts.
            //This looks like this:
            //[Old--Event][New--Event]
            //With a merge result of this:
            //[New--------------Event]

            //We have an existing event window start at the same time as our new window ends.
            //This looks like this:
            //[New--Event][Old--Event]
            //With a merge result of this:
            //[New--------------Event]

            //We have an existing event window that partially overlaps our event from the beginning
            //This looks like this:
            //[Old--Event]
            //   [New--Event]
            //With a merge result of this:
            //[New-----Event]

            //We have an existing event window that partially overlaps our event from the end
            //This looks like this:
            //      [Old--Event]
            //   [New--Event]
            //With a merge result of this:
            //   [New-----Event]

            //We have an existing event that's fully contained within our event window
            //This looks like this:
            //    [Old--Event]
            //[New-----------Event]
            //With a merge result of this:
            //[New-----------Event]

            //All overlapping event windows are deleted from the system, with our new/modified event being expanded to encompass the overlaps.


            var overlaps =
                uow.EventRepository.GetQuery()
                    .Where(ew =>
                        //Don't include deleted events
                        ew.EventStatus != EventState.Deleted &&

                        ew.Id != updatedEvent.Id && ew.CalendarID == updatedEvent.CalendarID &&
                            //If the existing event starts at or before our start date, and ends at or after our start date
                        ((ew.StartDate <= updatedEvent.StartDate && ew.EndDate >= updatedEvent.StartDate) ||
                            //If the existing event starts at or after our end date, and ends at or before our end date
                         (ew.StartDate <= updatedEvent.EndDate && ew.EndDate >= updatedEvent.EndDate) ||
                            //If the existing event is entirely within our new date
                         (ew.StartDate >= updatedEvent.StartDate && ew.EndDate <= updatedEvent.EndDate)
                         )).ToList();

            //We want to get the min/max start dates _including_ our new event window
            var fullSet = overlaps.Union(new[] { updatedEvent }).ToList();

            var newStart = fullSet.Min(ew => ew.StartDate);
            var newEnd = fullSet.Max(ew => ew.EndDate);

            //Delete all overlaps
            foreach (var overlap in overlaps)
                uow.EventRepository.Remove(overlap);

            //Potentially expand our new/modified event window
            updatedEvent.StartDate = newStart;
            updatedEvent.EndDate = newEnd;
        }

        //Renders a form to accept a new event
        private EventDO CreateNewEvent(IUnitOfWork uow, int? bookingRequestID, int calendarID, string start, string end)
        {
            if (!start.EndsWith("z"))
                throw new ApplicationException("Invalid date time");
            if (!end.EndsWith("z"))
                throw new ApplicationException("Invalid date time");

            //unpack the form data into an EventDO 
            EventDO createdEvent = new EventDO {BookingRequestID = bookingRequestID, CalendarID = calendarID};

            TimeSpan offset;
            if (bookingRequestID.HasValue)
            {
                var bookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestID);
                var guessedTimeZone = bookingRequest.Customer.GetOrGuessTimeZone();
                if (guessedTimeZone != null)
                    offset = bookingRequest.Customer.GetOrGuessTimeZone().GetUtcOffset(DateTime.Now);
                else
                    offset = DateTimeOffset.Now.Offset;
            }
            else
            {
                offset = DateTimeOffset.Now.Offset; //Legacy for now. Time slots don't care about timezones for now, as it's presented to the booker
            }

            //First, we need to parse the time into UTC.
            //UTC is the incorrect timezone (most likely), and we use it because our javascript library tries to use local time (the booker's local time)
            //So, we are sent the date, and told it's in UTC.
            //Then, we look at the headers of the original booking request
            //From the headers, we adjust our time by the senders offset

            //For example:
            //Customer sends an email at 8:45pm +7UTC
            //Customer asks for a meeting at '11am'
            //Booker clicks the 11am time slot
            //We recieve the date: 11am +0 UTC. However, the booker wants it to be 11am +7 UTC (We assume this based on the headers from their email)
            //We then subtract the bookers offset (7 hours)
            //We are then left with 4am +0 UTC, which is equivelant to 11am +7 UTC
            //Finally, we want to set the offset to the bookers offset. This part is merely cosmetic.
            //It allows us to show 11am +7:00, rather than 4am +0:00
            //Even though they are equivelant, we want to be local to the customer
            var startDateInitial = DateTimeOffset.ParseExact(start, DateStandardFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            var endDateInitial = DateTimeOffset.ParseExact(end, DateStandardFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            createdEvent.StartDate = startDateInitial.Subtract(offset).ToOffset(offset);
            createdEvent.EndDate = endDateInitial.Subtract(offset).ToOffset(offset);

            createdEvent.IsAllDay = createdEvent.StartDate.Equals(createdEvent.StartDate.Date) && createdEvent.StartDate.AddDays(1).Equals(createdEvent.EndDate);
            return createdEvent;
        }

        public ActionResult Edit(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                var curEventVM = _mappingEngine.Map<EventDO, EventVM>(eventDO);
                curEventVM.IsNew = false;
                return View(curEventVM);
            }
        }

        [HttpPost]
        public ActionResult ConfirmDelete(int eventID)
        {
            _event.Delete(eventID);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MoveEvent(int eventID, String newStart, String newEnd, bool requiresConfirmation = true, bool mergeEvents = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!newStart.EndsWith("z"))
                    throw new ApplicationException("Invalid date time");
                if (!newEnd.EndsWith("z"))
                    throw new ApplicationException("Invalid date time");

                var eventDO = uow.EventRepository.GetByKey(eventID);

                var evm = _mappingEngine.Map<EventDO, EventVM>(eventDO);

                evm.StartDate = DateTime.Parse(newStart, CultureInfo.InvariantCulture, 0).ToUniversalTime();
                evm.EndDate = DateTime.Parse(newEnd, CultureInfo.InvariantCulture, 0).ToUniversalTime();
                
                return ProcessChangedEvent(evm, 
                    confStatus: requiresConfirmation ? ConfirmationStatus.Unconfirmed : ConfirmationStatus.Unnecessary, 
                    mergeEvents: mergeEvents);
            }
        }

        public ActionResult ConfirmChanges(EventVM eventVM)
        {
            return View("~/Views/Event/ConfirmChanges.cshtml", eventVM);
        }

        [HttpGet]
        public ActionResult ProcessChangedEvent(EventVM curEventVM)
        {
            //Fix up serialization problem
            curEventVM.StartDate = DateTime.Parse(Request.Params["StartDate"], CultureInfo.InvariantCulture, 0).ToUniversalTime();
            curEventVM.EndDate = DateTime.Parse(Request.Params["EndDate"], CultureInfo.InvariantCulture, 0).ToUniversalTime();

            return ProcessChangedEvent(curEventVM, ConfirmationStatus.Confirmed, false);
        }

        public ActionResult ProcessChangedEvent(EventVM curEventVM, int confStatus, bool mergeEvents)
        {
            try
            {
                if (confStatus == ConfirmationStatus.Unconfirmed)
                {
                    return ConfirmChanges(curEventVM);
                }

                EventDO updatedEventInfo = _mappingEngine.Map<EventDO>(curEventVM);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    if (updatedEventInfo.Id == 0)
                        throw new ApplicationException("event should have been created and saved in #new, so Id should not be zero");
                    var curEventDO = uow.EventRepository.GetByKey(updatedEventInfo.Id);
                    if (curEventDO == null)
                        throw new EntityNotFoundException<EventDO>();
                    updatedEventInfo.Attendees = _attendee.ConvertFromString(uow, curEventVM.Attendees);

                    if (updatedEventInfo.Summary == null)
                        updatedEventInfo.Summary = String.Empty;

                    var newAttendees = _event.Update(uow, curEventDO, updatedEventInfo);

                    curEventDO.EventStatus = updatedEventInfo.Summary.Contains("DRAFT")
                        ? EventState.Draft
                        : EventState.ReadyForDispatch;

                    if (mergeEvents)
                        MergeTimeSlots(uow, curEventDO);

                    uow.SaveChanges();

                    foreach (var attendeeDO in updatedEventInfo.Attendees)
                    {
                        var user = new User();
                        var userDO = uow.UserRepository.GetOrCreateUser(attendeeDO.EmailAddress);
                        if (user.GetMode(userDO) == CommunicationMode.Delegate)
                        {
                            ObjectFactory.GetInstance<ITracker>()
                                .Track(userDO, "Customer", "InvitedAsPreCustomerAttendee",
                                    new Dictionary<string, object>
                                    {
                                        {"BookingRequestId", curEventDO.BookingRequestID},
                                        {"EventID", curEventDO.Id}
                                    });
                        }
                    }

                    if (!curEventVM.BookingRequestID.HasValue)
                        return Json(true, JsonRequestBehavior.AllowGet);

                    var emailController = new EmailController();

                    var currCreateEmailVM = new CreateEmailVM
                    {
                        ToAddresses = updatedEventInfo.Attendees.Select(a => a.EmailAddress.Address).ToList(),
                        Subject = "*** Automatically Generated ***",
                        RecipientsEditable = false,
                        BCCHidden = true,
                        CCHidden = true,
                        SubjectEditable = false,
                        HeaderText = String.Format("Your event has been created. Would you like to send the emails now?"),
                        BodyPromptText = "Enter some additional text for your recipients",
                        Body = "",
                        BodyRequired = false,
                        BookingRequestId = curEventVM.BookingRequestID.Value
                    };

                    return emailController.DisplayEmail(Session, currCreateEmailVM,
                        (subUow, emailDO) =>
                        {
                            var subEventDO = subUow.EventRepository.GetByKey(curEventDO.Id);
                            _event.GenerateInvitations(subUow, subEventDO, newAttendees, emailDO.HTMLText);
                            subEventDO.EventStatus = EventState.DispatchCompleted;
                            subUow.SaveChanges();
                            return Json(true);
                        }
                    );
                }   
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error saving event", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
            
        }

        public ActionResult DeleteEvent(int eventID, bool requiresConfirmation = true)
        {
            if (requiresConfirmation)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var eventDO = uow.EventRepository.GetByKey(eventID);
                    return View(_mappingEngine.Map<EventDO, EventVM>(eventDO));
                }
            }

            return ConfirmDelete(eventID);
        }
    }
}