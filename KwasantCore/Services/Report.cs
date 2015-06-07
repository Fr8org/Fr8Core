using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Services
{
    public class Report
    {
        public const string DateStandardFormat = @"yyyy-MM-ddTHH\:mm\:ss.fffffff"; //This allows javascript to parse the date properly
        private readonly User _user;
        private readonly Email _email;
        private Dictionary<string, string> _dataUrlMappings;

        public Report() 
        {
            _user = new User();
            _email = new Email();
        }

        
        public object Generate(IUnitOfWork uow, DateRange dateRange, string type, int start,
            int length, out int recordcount)
        {
            recordcount = 0;
            switch (type)
            {
                case "alllogs":
                    return ShowAllLogs(uow, dateRange, start,
             length, out recordcount);
                case "usage":
                    return GenerateUsageReport(uow, dateRange, start,
             length, out recordcount);
                case "incident":
                    return ShowAllIncidents(uow, dateRange, start,
            length, out recordcount);
                case "fiveRecentIncident":
                    return ShowMostRecent5Incidents(uow, out recordcount);
                case "showBookerThroughput":
                    return ShowBookerThroughput(uow, dateRange, start,
             length, out recordcount);
                case "showBRResponsiveness":
                    return ShowBRResponsiveness(uow, dateRange, start,
             length, out recordcount);
            }
            return this;
        }

        private IList ShowAllLogs(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var logs = uow.LogRepository.GetQuery().WhereInDateRange(e => e.CreateDate, dateRange);

            count = logs.Count();

            return logs
                .OrderByDescending(e => e.CreateDate)
                .Skip(start)
                .Take(length)
                .AsEnumerable()
                .Select(l => new
                {
                    Date = l.CreateDate.ToString(DateStandardFormat),
                    l.Name,
                    l.Level,
                    l.Message
                }).ToList();

        }

        private object GenerateUsageReport(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            _dataUrlMappings = new Dictionary<string, string>();
            _dataUrlMappings.Add("BookingRequest", "/Dashboard/Index/");
            _dataUrlMappings.Add("Email", "/Dashboard/Index/");
            _dataUrlMappings.Add("User", "/User/Details?userID=");


            var factDO = uow.FactRepository.GetQuery().WhereInDateRange(e => e.CreateDate, dateRange);

            count = factDO.Count();

            return factDO
                .OrderByDescending(e => e.CreateDate)
                .Skip(start)
                .Take(length)
                .AsEnumerable()
                .Select(
                    f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Status = f.Status,
                            Data = AddClickability(f.Data),
                            CreateDate = f.CreateDate.ToString(DateStandardFormat),
                        })
                .ToList();

        }

        private IList ShowAllIncidents(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var incidentDO = uow.IncidentRepository.GetQuery().WhereInDateRange(e => e.CreateDate, dateRange);

            count = incidentDO.Count();

            return incidentDO
                .OrderByDescending(e => e.CreateDate)
                .Skip(start)
                .Take(length)
                .AsEnumerable()
                .Select(
                    f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Data = f.Data,
                            CreateDate = f.CreateDate.ToString(DateStandardFormat),
                            ObjectId = f.ObjectId

                        })
                .ToList();
        }

        private IList ShowMostRecent5Incidents(IUnitOfWork uow, out int count)
        {
            var incidentDO = uow.IncidentRepository.GetQuery().OrderByDescending(x => x.CreateDate).Take(5);

            count = incidentDO.Count();
            return incidentDO
                .AsEnumerable()
                .Select(
                    f => new
                        {
                            Id = f.Id,
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Data = f.Data,
                            CreateDate = f.CreateDate.ToString(DateStandardFormat),

                        })
                .ToList();

        }
        
        public object GenerateHistoryReport(IUnitOfWork uow, DateRange dateRange, string primaryCategory, string strBookingRequestId)
        {
            return uow.HistoryRepository.GetQuery()
                .Where(e => e.ObjectId == strBookingRequestId
                            && e.PrimaryCategory == primaryCategory)
                .WhereInDateRange(e => e.CreateDate, dateRange)
                .OrderByDescending(e => e.CreateDate)
                .AsEnumerable()
                .Select(
                    e =>
                    new
                        {
                            PrimaryCategory = e.PrimaryCategory,
                            SecondaryCategory = e.SecondaryCategory,
                            Activity = e.Activity,
                            Status = e.Status,
                            Data = e.Data,
                            CreateDate = e.CreateDate.ToString(DateStandardFormat),
                        })
                .ToList();
        }

        public object GenerateHistoryByBookingRequestId(IUnitOfWork uow, int bookingRequestId)
        {
            var strBookingRequestId = bookingRequestId.ToString(CultureInfo.InvariantCulture);
            return uow.HistoryRepository.GetQuery()
                .Where(e => e.ObjectId == strBookingRequestId)
                .OrderByDescending(e => e.CreateDate)
                .AsEnumerable()
                .Select(
                    e =>
                    new
                        {
                            PrimaryCategory = e.PrimaryCategory,
                            Activity = e.Activity,
                            Status = e.Status,
                            Data = e.Data,
                            CreateDate = e.CreateDate.ToString(DateStandardFormat),
                            SecondaryCategory = e.SecondaryCategory,
                            BookerId = e.BookerId
                        })
                .ToList();
        }

        private string AddClickability(string originalData)
        {
            if (originalData != null)
            {
                //This try-catch is to move on, even if something generates error, this is because right now we don't have consistency in our "Data" field
                //so in case of error it just return the original data.
                try
                {
                    string objectType = originalData.Split(' ')[0].ToString();
                    var splitedData = originalData.Split(':')[1];
                    string objectId = splitedData.Substring(0, splitedData.IndexOf(","));
                    string clickableLink = GetClickableLink(objectType, objectId);

                    originalData = originalData.Replace(objectId, clickableLink);
                }
                catch { }
            }
            return originalData;
        }

        private string GetClickableLink(string objectType, string objectId)
        {
            if (objectType == "Email")
            {
                string bookingRequestId = _email.FindEmailParentage(Convert.ToInt32(objectId));
                if (bookingRequestId != null)
                    return string.Format("<a style='padding-left:3px' target='_blank' href='{0}{1}'>{2}</a>", _dataUrlMappings[objectType], bookingRequestId, objectId);
            }
            if (objectType == "User")
            {
                string userId = _user.GetUserId(objectId);
                return string.Format("<a style='padding-left:3px' target='_blank' href='{0}{1}'>{2}</a>", _dataUrlMappings[objectType], userId, objectId);
            }
            return string.Format("<a style='padding-left:3px' target='_blank' href='{0}{1}'>{2}</a>", _dataUrlMappings[objectType], objectId, objectId);
        }

        private object ShowBookerThroughput(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            //Booker _booker = new Booker();
            var incidentDO = uow.IncidentRepository.GetQuery()
                .Where(e => e.PrimaryCategory == "BookingRequest" && e.Activity == "MarkedAsProcessed")
                .WhereInDateRange(e => e.CreateDate, dateRange)
                .GroupBy(e => e.BookerId)
                .OrderByDescending(e => e.Key);


            count = incidentDO.Count();

            return incidentDO
                .Skip(start).Take(length)
                .AsEnumerable()
                .Select(e => new
                    {
                       // BRNameAndCount =
                               //  string.Format("{0} marked as processed {1} distinct BRs", _booker.GetName(uow, e.Key), e.Count()),
                    })
                .ToList();

        }

        private object ShowBRResponsiveness(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var incidentDO = uow.IncidentRepository.GetQuery()
                .Where(e => e.PrimaryCategory == "BookingRequest" && e.Activity == "Checkout")
                .WhereInDateRange(e => e.CreateDate, dateRange)
                .OrderByDescending(e => e.CreateDate);
                
            count = incidentDO.Count();
            return incidentDO
                .Skip(start).Take(length)
                .AsEnumerable()
                .Select(e => new
                    {
                        ObjectId = e.ObjectId,
                        TimeToProcess = e.Data.Substring(e.Data.LastIndexOf(':') + 1),
                    })
                .ToList();
        }
    }
}
