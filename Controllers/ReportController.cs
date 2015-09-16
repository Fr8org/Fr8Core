using System.Web.Mvc;
using Data.Interfaces;
using Core.Managers;
using Core.Services;
using StructureMap;
using Utilities;
using Data.Entities;
using System.Collections.Generic;
using System;
using Utilities.Logging;



namespace Web.Controllers
{
    [DockyardAuthorize(Roles = "Booker")]
    public class ReportController : Controller
    {
        //private DataTablesPackager _datatables;
        private Report _report;
        private JsonPackager _jsonPackager;

        public ReportController()
        {
            _report = new Report();
            _jsonPackager = new JsonPackager();
        }

        //
        // GET: /Report/
        public ActionResult Index(string type)
        {
            ViewBag.type = type;
            switch (type)
            {
                case "usage" :
                    ViewBag.Title = "Usage Report";
                    break;
                case "incident":
                    ViewBag.Title = "Incident Report";
                    break;
            }
            return View();
        }

        [HttpPost]
        public ActionResult ShowReport(string queryPeriod, string type, int? draw, int start, int length)
        {
            DateRange dateRange = DateUtility.GenerateDateRange(queryPeriod);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                int recordcount;
                var report = _report.Generate(uow, dateRange, type, start, length, out recordcount);
                var jsonResult = Json(new
                {
                    draw = draw,
                    recordsTotal = recordcount,
                    recordsFiltered = recordcount,
                    data = _jsonPackager.Pack(report)
                });

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        //Display View "History"
        public ActionResult History()
        {
            return View("History");
        }

        [HttpPost]
        public ActionResult ShowHistoryReport(string primaryCategory, string bookingRequestId, string queryPeriod)
        {
            DateRange dateRange = DateUtility.GenerateDateRange(queryPeriod);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var historyReport = _report.GenerateHistoryReport(uow, dateRange, primaryCategory, bookingRequestId);
                var jsonResult = Json(_jsonPackager.Pack(historyReport));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        //Display partial view "_History" on new window.
        public ActionResult HistoryByBookingRequestId(int bookingRequestID)
        {
            ViewBag.bookingRequestID = bookingRequestID;
            return View("_History");
        }

        public ActionResult ShowHistoryByBookingRequestId(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var historyByBRId = _report.GenerateHistoryByBookingRequestId(uow, bookingRequestId);
                var jsonResult = Json(_jsonPackager.Pack(historyByBRId), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
             }
        }

        
        public JsonResult Facts()
        {
            JsonResult jsonResult=new JsonResult();
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    List<FactDO> factDOList = _report.GetFacts(uow);
                    jsonResult = Json(factDOList,JsonRequestBehavior.AllowGet);                  
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return jsonResult;
        }

       
        public JsonResult Incidents()
        {
            JsonResult jsonResult = new JsonResult();
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    List<IncidentDO> incidentList = _report.GetIncidents(uow);
                    jsonResult = Json(incidentList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return jsonResult;
        }


        public ActionResult ShowFacts()
        {
            return View();
        }

        public ActionResult ShowIncidents()
        {
            return View();
        }
       
	}
}