using System;
using System.Globalization;
using System.Web.Mvc;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events;
using DayPilot.Web.Mvc.Events.Common;
using DayPilot.Web.Mvc.Events.Scheduler;
using DayPilot.Web.Mvc.Utils;

namespace KwasantWeb.Controllers
{
    [HandleError]
    public class SchedulerController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CssContinue()
        {
            return View();
        }

        public ActionResult AutoCellWidth()
        {
            return View();
        }

        public ActionResult ScaleHours()
        {
            return View();
        }

        public ActionResult ScaleDays()
        {
            return View();
        }

        public ActionResult ScaleWeeks()
        {
            return View();
        }

        public ActionResult ScaleMonths()
        {
            return View();
        }

        public ActionResult ScaleYears()
        {
            return View();
        }

        public ActionResult ThemeTransparent()
        {
            return View();
        }
        
        public ActionResult Notify()
        {
            return View();
        }
        
        public ActionResult ThemeSilver()
        {
            return RedirectToAction("ThemeTraditional");
        }
        
        public ActionResult ThemeWhite()
        {
            return View();
        }
        
        public ActionResult ThemeGreen()
        {
            return View();
        }

        public ActionResult Theme8()
        {
            return View();
        }

        public ActionResult ThemeGreenWithDurationBar()
        {
            return RedirectToAction("ThemeGreen");
        }

        public ActionResult ThemeBlue()
        {
            return View();
        }
        
        public ActionResult ThemeTraditional()
        {
            return View();
        }
        
        public ActionResult ActiveAreas()
        {
            return View();
        }        

        public ActionResult PercentComplete()
        {
            return View();
        }        

        public ActionResult RowHeaderAutoFit()
        {
            return View();
        }        
        
        public ActionResult PreventParentUsage()
        {
            return View();
        }       
        
        public ActionResult LimitEventMoving()
        {
            return View();
        }
        
        public ActionResult EventMoveToPosition()
        {
            return View();
        }

        public ActionResult Gantt()
        {
            return View();
        }

        public ActionResult ExternalDragDrop()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult RecurringEvents()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }

        public ActionResult Scrolling()
        {
            return View();
        }

        public ActionResult Crosshair()
        {
            return View();
        }

        public ActionResult ContextMenu()
        {
            return View();
        }

        public ActionResult AutoRefresh()
        {
            return View();
        }

        public ActionResult DynamicEventLoading()
        {
            return View();
        }

        public ActionResult DynamicTreeLoading()
        {
            return View();
        }

        public ActionResult RowHeaderColumns()
        {
            return View();
        }
        
        public ActionResult RowHeight()
        {
            return View();
        }

        public ActionResult EventSelecting()
        {
            return View();
        }

        public ActionResult TimeHeaders()
        {
            return View();
        }

        public ActionResult Hijri()
        {
            return View();
        }        

        public ActionResult Hebrew()
        {
            return View();
        }        

        public ActionResult Japanese()
        {
            return View();
        }        

        public ActionResult Korean()
        {
            return View();
        }        

        public ActionResult Taiwan()
        {
            return View();
        }        

        public ActionResult ThaiBuddhist()
        {
            return View();
        }        


        public ActionResult Backend()
        {
            return new Dps().CallBack(this);
        }

        //public ActionResult Edit(string id)
        //{
        //    var e = new EventManager(this).Get(id) ?? new EventManager.Event();
        //    return View(e);
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Edit(FormCollection form)
        //{
        //    new EventManager(this).EventEdit(form["Id"], form["Text"]);
        //    return JavaScript(SimpleJsonSerializer.Serialize(true));
        //}

        class Dps : DayPilotScheduler
        {
            //private bool useViewPort;
            Random random = new Random();

            protected override void OnInit(InitArgs ea)
            {
                //StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                //Days = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);

                if (Id == "dps_rowheadercolumns") // Scheduler/RowHeaderColumns
                {
                    foreach (Resource r in Resources)
                    {
                        foreach (Resource c in r.Children)
                        {
                            c.Columns.Add(new ResourceColumn("Col A"));
                            c.Columns.Add(new ResourceColumn("Col B"));
                        }
                    }
                }
                if (Id == "dps_jquery") // Scheduler/JQuery
                {
                    for (char i = 'A'; i < 'Z'; i++)
                    {
                        Resources.Add("" + i, i.ToString());
                    }
                }
                if (Id == "dps_hebrew")
                {
                    DateTimeFormatInfo = new CultureInfo("he-il", false).DateTimeFormat;
                    DateTimeFormatInfo.Calendar = new HebrewCalendar();

                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }
                if (Id == "dps_hijri")
                {
                    DateTimeFormatInfo = new CultureInfo("ar-sa", false).DateTimeFormat;
                    DateTimeFormatInfo.Calendar = new HijriCalendar();

                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }
                if (Id == "dps_japanese")
                {
                    DateTimeFormatInfo = new CultureInfo("ja-jp", false).DateTimeFormat;
                    DateTimeFormatInfo.Calendar = new JapaneseCalendar();

                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }
                if (Id == "dps_korean")
                {
                    DateTimeFormatInfo = new CultureInfo("ko-kr", false).DateTimeFormat;
                    DateTimeFormatInfo.Calendar = new KoreanCalendar();

                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }
                if (Id == "dps_taiwan")
                {
                    DateTimeFormatInfo = new CultureInfo("zh-tw", false).DateTimeFormat;
                    DateTimeFormatInfo.Calendar = new TaiwanCalendar();

                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }
                if (Id == "dps_thaibuddhist")
                {
                    DateTimeFormatInfo = new CultureInfo("th-th", false).DateTimeFormat;
                    DateTimeFormatInfo.Calendar = new ThaiBuddhistCalendar();

                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }

                if (Id == "dps_scrolling" || Id == "dps_dynamic")
                {
                    int year = Calendar.GetYear(DateTime.Today);
                    StartDate = new DateTime(year, 1, 1, Calendar);
                    Days = Calendar.GetDaysInYear(year);
                }
                
                ScrollTo(DateTime.Today);

                UpdateWithMessage("Welcome!", CallBackUpdateType.Full);
            }

            protected override void OnEventSelect(EventSelectArgs e)
            {
                if (e.Event.Value == "1" && e.Change == EventSelectChange.Selected)
                {
                    SelectedEvents.Add(EventInfo.Create("13"));
                }
                Update();
            }

            protected override void OnLoadNode(LoadNodeArgs e)
            {
                Resource child = new Resource("Test", Guid.NewGuid().ToString());
                child.DynamicChildren = true;

                e.Resource.Children.Add(child);
                e.Resource.Expanded = true;

                Update(CallBackUpdateType.Full);
            }

            //protected override void OnCommand(CommandArgs e)
            //{
            //    switch (e.Command)
            //    {
            //        case "filter":
            //            Update();
            //            break;
            //        case "refresh":
            //            UpdateWithMessage("Refreshed");
            //            break;
            //        case "next":
            //            StartDate = StartDate.AddYears(1);
            //            Days = Year.Days(StartDate);
            //            Update(CallBackUpdateType.Full);
            //            break;
            //        case "previous":
            //            StartDate = StartDate.AddYears(-1);
            //            Days = Year.Days(StartDate);
            //            Update(CallBackUpdateType.Full);
            //            break;
            //        case "this":
            //            StartDate = Year.First(DateTime.Today);
            //            Days = Year.Days(StartDate);
            //            Update(CallBackUpdateType.Full);
            //            break;
            //        case "selected":
            //            if (SelectedEvents.Count > 0)
            //            {
            //                EventInfo ei = SelectedEvents[0];
            //                SelectedEvents.RemoveAt(0);
            //                UpdateWithMessage("Event removed from selection: " + ei.Text);
            //            }

            //            break;
            //        case "delete":
            //            string id = (string)e.Data["id"];
            //            new EventManager(Controller).EventDelete(id);
            //            Update(CallBackUpdateType.EventsOnly);
            //            break;
            //    }
            //}

            protected override void  OnBeforeCellRender(BeforeCellRenderArgs e)
            {
                if (Id == "dps_parents")
                {
                    if (Resources.FindById(e.ResourceId).Children.Count > 0)
                    {
                        e.BackgroundColor = "white";
                    }
                }

                if (e.Start.Day == 1)
                {
                    e.CssClass = "red";
                }
            }

            protected override void OnBeforeResHeaderRender(BeforeResHeaderRenderArgs e)
            {
                if (Id == "dps_areas")
                {
                    e.Areas.Add(new Area().Width(17).Bottom(1).Right(0).Top(0).CssClass("resource_action_menu").Html("<div><div></div></div>").JavaScript("alert(e.Value);"));
                }

                if (e.Columns.Count > 0)
                {
                    e.Columns[0].Html = "10 seats";
                }

            }

            protected override void OnEventBubble(EventBubbleArgs e)
            {
                e.BubbleHtml = "Event details for id: " + e.Id + "<br/>" + e.Start + " " + e.End;
            }

            //protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            //{
            //    new EventManager(Controller).EventCreate(e.Start, e.End, "Default name", e.Resource);
            //    UpdateWithMessage("New event created", CallBackUpdateType.EventsOnly);
            //}

            //protected override void OnEventMove(EventMoveArgs e)
            //{
            //    if (new EventManager(Controller).Get(e.Id) != null)
            //    {
            //        new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd, e.NewResource);
            //    }
            //    else // external drag&drop
            //    {
            //        new EventManager(Controller).EventCreate(e.NewStart, e.NewEnd, e.Text, e.NewResource, e.Id);
            //    }
            //    if (Id == "dps_position")
            //    {
            //        UpdateWithMessage("Moved to position: " + e.Position);
            //    }
            //    else
            //    {
            //        UpdateWithMessage("Event moved.");
            //    }

            //}

            //protected override void OnEventResize(EventResizeArgs ea)
            //{
            //    new EventManager(Controller).EventMove(ea.Id, ea.NewStart, ea.NewEnd, ea.Resource);
            //    Update();
            //}

            protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
            {
                if (Id == "dps_limit")
                {
                    int id = 0;
                    int.TryParse(e.Id, out id);

                    if (id % 2 == 0)
                    {
                        e.DurationBarColor = "red";
                        e.EventMoveVerticalEnabled = false;
                    }
                    else
                    {
                        e.DurationBarColor = "blue";
                        e.EventMoveHorizontalEnabled = false;
                    }
                }
                else if (Id == "dps_areas")
                {
                    e.Areas.Add(new Area().Width(17).Bottom(9).Right(2).Top(3).CssClass("event_action_delete").JavaScript("dps_areas.commandCallBack('delete', {id:e.value() });"));
                    e.Areas.Add(new Area().Width(17).Bottom(9).Right(19).Top(3).CssClass("event_action_menu").ContextMenu("menu"));
                }
                else if (Id == "dps_complete")
                {
                    int complete = random.Next(100);
                    e.PercentComplete = complete;

                    string cs = String.Format("{0}%", complete);
                    e.Html = cs;

                }

                if (e.Recurrent)
                {
                    e.Html += " (R)";
                }

            }

            //protected override void OnEventMenuClick(EventMenuClickArgs e)
            //{
            //    switch (e.Command)
            //    {
            //        case "Delete":
            //            new EventManager(Controller).EventDelete(e.Id);
            //            Update();
            //            break;
            //    }
            //}

            protected override void OnEventClick(EventClickArgs e)
            {
                if (Id == "dps_message")
                {
                    UpdateWithMessage("Event clicked: " + e.Text);
                }
            }

            protected override void OnBeforeTimeHeaderRender(BeforeTimeHeaderRenderArgs e)
            {
                if (Id == "dps_timeheaders")
                {
                    if (e.Level == 1)
                    {
                        DateTime monday = Week.Monday(e.Start, ResolvedWeekStart);
                        e.InnerHtml = String.Format("Week {0}", Week.WeekNrISO8601(monday));
                    }
                }
            }

            protected override void OnIncludeCell(IncludeCellArgs e)
            {
                /*
                if (e.Start.DayOfWeek == DayOfWeek.Saturday || e.Start.DayOfWeek == DayOfWeek.Sunday)
                {
                    e.Visible = false;
                }
                 * */

            }

            protected override void OnResourceExpand(ResourceExpandArgs args)
            {
                UpdateWithMessage("expanded " + args.Resource.Id);
            }

            protected override void OnNotify(NotifyArgs e)
            {
                foreach(DayPilotArgs a in e.Queue)
                {
                    if (a is EventUpdateArgs)
                    {
                        EventUpdateArgs updateArgs = (EventUpdateArgs) a;
                        string id = updateArgs.Event.Value;
                        string newText = updateArgs.New.Text;
                        // update the db
                    }
                }
            }

            protected override void OnScroll(ScrollArgs e)
            {
                Update(CallBackUpdateType.EventsOnly);
            }

            protected override void OnBeforeEventRecurrence(BeforeEventRecurrenceArgs e)
            {
            }

            //protected override void OnFinish()
            //{
            //    // only load the data if an update was requested by an Update() call
            //    if (UpdateType == CallBackUpdateType.None)
            //    {
            //        return;
            //    }

            //    if (HeaderColumns.Count == 0)
            //    {
            //        CornerHtml = String.Format("<div style='padding:5px; font-weight: bold; font-size:22px; text-align:center'>{0}</div>", Calendar.GetYear(StartDate));
            //    }

            //    DateTime start = DynamicLoading ? ViewPort.Start : StartDate;
            //    DateTime end = DynamicLoading ? ViewPort.End : EndDate;

            //    if (Id == "dps_recurring")
            //    {
            //        Events = new EventManager(Controller, "recurring").FilteredData(start, end, (string)ClientState["filter"]).AsEnumerable();
            //        DataRecurrenceField = "recurrence";
            //    }
            //    else
            //    {
            //        Events = new EventManager(Controller).FilteredData(start, end, (string)ClientState["filter"]).AsEnumerable();
            //    }

            //    //Separators.Clear();
            //    //Separators.Add(DateTime.Now, Color.Red);


            //    DataStartField = "start";
            //    DataEndField = "end";
            //    DataTextField = "text";
            //    DataIdField = "id";
            //    DataResourceField = "resource";

            //    DataTagFields = "id, text, resource";
            //}

        }

    }
}
