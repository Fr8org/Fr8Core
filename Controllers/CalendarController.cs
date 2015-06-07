using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.Controllers.External.DayPilot;
using KwasantWeb.Controllers.External.DayPilot.Providers;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [HandleError]
    [KwasantAuthorize(Roles = "Booker")]
    public class CalendarController : Controller
    {

        #region "Action"

        public ActionResult GetSpecificCalendar(int calendarID)
        {
            if (calendarID <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var calendarRepository = uow.CalendarRepository;
                var calendarDO = calendarRepository.GetByKey(calendarID);
                if (calendarDO == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                return View("~/Views/Negotiation/EventWindows.cshtml", new EventWindowVM
                {
                    ActiveCalendarID = calendarID,
                    ClickEditEnabled = false,
                    MergeEvents = true,
                    RequiresConfirmation = false
                });
            }
        }

        public ActionResult GetNegotiationCalendars(int calendarID, String defaultEventDescription = null)
        {
            if (calendarID <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var calendarRepository = uow.CalendarRepository;
                var calendarDO = calendarRepository.GetByKey(calendarID);
                if (calendarDO == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                IEnumerable<CalendarDO> calendarsViaNegotiationRequest;
                if (calendarDO.Negotiation != null)
                {
                    calendarsViaNegotiationRequest = calendarDO.Negotiation.Calendars;
                    var recipientAddresses = calendarDO.Negotiation.Attendees.Select(r => r.EmailAddress) //Get email addresses for each recipient
                        .Select(a => uow.UserRepository.GetOrCreateUser(a)).Where(u => u != null).SelectMany(u => u.Calendars).ToList(); //Grab the user from the email and find their calendars

                    IEnumerable<CalendarDO> bookingRequestCustomerCalendars = new List<CalendarDO>();
                    if (calendarDO.Negotiation.BookingRequest != null && calendarDO.Negotiation.BookingRequest.Customer != null)
                        bookingRequestCustomerCalendars = calendarDO.Negotiation.BookingRequest.Customer.Calendars;

                    //Grab the user from the email and find their calendars
                    calendarsViaNegotiationRequest = calendarsViaNegotiationRequest.Union(recipientAddresses).Union(bookingRequestCustomerCalendars);
                }
                else
                {
                    calendarsViaNegotiationRequest = new List<CalendarDO>();
                }

                return View("~/Views/Negotiation/EventWindows.cshtml", new EventWindowVM
                {
                    LinkedCalendarIDs = calendarsViaNegotiationRequest.Select(c => c.Id).Union(new[] { calendarID }).Distinct().ToList(),
                    ActiveCalendarID = calendarID,
                    ClickEditEnabled = false,
                    MergeEvents = true,
                    RequiresConfirmation = false,
                    DefaultEventDescription = defaultEventDescription
                });
            }
        }

        #endregion "Action"

        #region "DayPilot-Related Methods"
        public ActionResult Day(string calendarIDs)
        {
            var ids = calendarIDs.Split(',').Where(c => !String.IsNullOrEmpty(c)).Select(int.Parse).ToArray();
            return new KwasantCalendarController(new EventDataProvider(true, ids)).CallBack(this);
        }

        public ActionResult Month(string calendarIDs)
        {
            var ids = calendarIDs.Split(',').Where(c => !String.IsNullOrEmpty(c)).Select(int.Parse).ToArray();
            return new KwasantMonthController(new EventDataProvider(true, ids)).CallBack(this);
        }

        public ActionResult Navigator(string calendarIDs)
        {
            var ids = calendarIDs.Split(',').Where(c => !String.IsNullOrEmpty(c)).Select(int.Parse).ToArray();
            return new KwasantNavigatorControl(new EventDataProvider(true, ids)).CallBack(this);
        }

        public ActionResult Rtl()
        {
            return View();
        }
        public ActionResult Columns50()
        {
            return View();
        }
        public ActionResult Height100Pct()
        {
            return View();
        }

        public ActionResult Notify()
        {
            return View();
        }

        public ActionResult Crosshair()
        {
            return View();
        }

        public ActionResult ThemeBlue()
        {
            return View();
        }

        public ActionResult ThemeGreen()
        {
            return View();
        }

        public ActionResult ThemeWhite()
        {
            return View();
        }

        public ActionResult ThemeTraditional()
        {
            return View();
        }

        public ActionResult ThemeTransparent()
        {
            return View();
        }

        public ActionResult TimeHeaderCellDuration()
        {
            return View();
        }

        public ActionResult ActiveAreas()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult HeaderAutoFit()
        {
            return View();
        }

        public ActionResult ExternalDragDrop()
        {
            return View();
        }

        public ActionResult EventArrangement()
        {
            return View();
        }

        public ActionResult AutoRefresh()
        {
            return View();
        }

        public ActionResult Today()
        {
            return View();
        }

        public ActionResult DaysResources()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        public ActionResult ContextMenu()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }

        public ActionResult DayRange()
        {
            return View();
        }

        public ActionResult EventSelecting()
        {
            return View();
        }

        public ActionResult AutoHide()
        {
            return View();
        }

        public ActionResult GoogleLike()
        {
            return View();
        }

        public ActionResult RecurringEvents()
        {
            return View();
        }

        public ActionResult ThemeSilver()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult ThemeGreenWithBar()
        {
            return RedirectToAction("ThemeGreen");
        }

        public ActionResult Outlook2000()
        {
            return RedirectToAction("ThemeTraditional");
        }

        #endregion "DayPilot-Related Methods"

        #region "Quick Copy Methods"
        [HttpPost]
        public ActionResult ProcessQuickCopy(string copyType, string selectedText)
        {
            string value = (new Calendar()).ProcessQuickCopy(copyType, selectedText);
            string status = "valid";
            if (value == "Invalid Selection") { status = "invalid"; }
            var jsonResult = Json(new { status = status, value = value, copytype = copyType });
            return jsonResult;
        }
        #endregion "Quick Copy Methods"
    }
}
