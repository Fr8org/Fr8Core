using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using Utilities;

namespace Daemons
{
    /// <summary>
    /// This Daemon looks for new booking requests, or unprocessed booking requests based on TrackingStatusDO.
    /// New booking requests are sent to the communication manager, which will then send off emails/smses to specific people
    /// </summary>
    public class FreshnessMonitor : Daemon<FreshnessMonitor>
    {
        private readonly IConfigRepository _configRepository;
        //private readonly IBookingRequest _br;
        private readonly IExpectedResponse _er;

        public FreshnessMonitor()
            : this(ObjectFactory.GetInstance<IConfigRepository>())
        {
        }

        private FreshnessMonitor(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
          //  _br = ObjectFactory.GetInstance<IBookingRequest>();
         //   _er = ObjectFactory.GetInstance<IExpectedResponse>();
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromMinutes(5).TotalMilliseconds; }
        }

        protected override void Run()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {  
                //TimeOutStaleCheckouts(uow);

                //TimeOutStaleReserved(uow);

                //MonitorStaleBRs(uow);

                DetectStaleExpectedResponses(uow);

                uow.SaveChanges();
            }
        }

        //private void TimeOutStaleReserved(IUnitOfWork uow)
        //{
        //    //Event: A reserved BR has timed out
        //    //Action: make BR available to other bookers than preferred one
        //    double maxBRReservationPeriodMinutes = Convert.ToDouble(_configRepository.Get<string>("MaxBRReservationPeriod"));

        //    DateTimeOffset reservationTimeLimit = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(maxBRReservationPeriodMinutes));
        //    List<BookingRequestDO> timedOutBRList = uow.BookingRequestRepository.GetQuery()
        //        .Where(x => x.State == BookingRequestState.NeedsBooking &&
        //            x.Availability != BookingRequestAvailability.ReservedPB &&
        //            x.BookerID == null &&
        //            x.PreferredBookerID != null &&
        //            x.LastUpdated < reservationTimeLimit).ToList();
        //    foreach (var br in timedOutBRList)
        //    {
        //      //  _br.ReservationTimeout(uow, br);
        //        LogSuccess("Booking request reservation timed out");
        //    }
        //}

        //private void TimeOutStaleCheckouts(IUnitOfWork uow)
        //{
        //    //Event: A checkedout BR has timed out
        //    //Action: change BR status "CheckedOut" to "Unprocessed"
        //    double maxBRIdleMinutes = Convert.ToDouble(_configRepository.Get<string>("MaxBRIdle"));

           
        //    DateTimeOffset idleTimeLimit = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(maxBRIdleMinutes));
        //    IEnumerable<BookingRequestDO> staleBRList = uow.BookingRequestRepository.GetQuery().Where(x => x.State == BookingRequestState.Booking && x.LastUpdated < idleTimeLimit);
        //    foreach (var br in staleBRList)
        //    {
        //        _br.Timeout(uow, br);
        //        LogSuccess("Booking request timed out");
        //    }
        //}

        //private void MonitorStaleBRs(IUnitOfWork uow)
        //{
        //    //Event: BR's are Not getting Processed
        //    //Action: Alert specified targets via SMS
        //    var currentTime = DateTimeOffset.Now;
        //    var smsIntervalMin = _configRepository.Get("MonitorStaleBRPeriod", 60);
        //    if ((int)currentTime.TimeOfDay.TotalMinutes % smsIntervalMin < TimeSpan.FromMilliseconds(WaitTimeBetweenExecution).TotalMinutes)
        //    {
        //        var notification = ObjectFactory.GetInstance<INotification>();
        //        if (notification.IsInNotificationWindow("ThroughputCheckingStartTime", "ThroughputCheckingEndTime"))
        //        {
        //            //The current time is in the specified time range (currently daytime, PST)....
        //            //We have to compare with a datetime - EF doesn't support operations like subtracts of datetimes, checking by ticks, etc.
        //            //The below creates a datetime which represents thirty minutes ago. Anything 'less' than this time is older than 30 minutes.
        //            var thirtyMinutesAgo = currentTime.Subtract(new TimeSpan(0, 0, 30, 0));

        //            //Per Wiki: https://maginot.atlassian.net/wiki/display/SH/Processing+Delay+Alerts
        //            //Once every hour, the monitor should query for BookingRequests that have status Unprocessed or CheckOut and are at least 30 minutes old, as measured by comparing the current time to the DateCreated time.
        //            var oldBookingRequests =
        //                uow.BookingRequestRepository.GetQuery()
        //                    .Where(
        //                        br =>
        //                        (br.State == BookingRequestState.NeedsBooking || br.State == BookingRequestState.Booking) &&
        //                        br.CreateDate <= thirtyMinutesAgo)
        //                    .ToArray();

        //            if (oldBookingRequests.Any())
        //            {
        //                AlertManager.StaleBookingRequestsDetected(oldBookingRequests);
        //                LogSuccess(oldBookingRequests.Length + " Booking requests are over-due by 30 minutes.");
        //            }
        //        }
        //    }
        //}

        private void DetectStaleExpectedResponses(IUnitOfWork uow)
        {
            double expectedResponseActiveDurationMinutes = Convert.ToDouble(_configRepository.Get<string>("ExpectedResponseActiveDuration"));

            DateTimeOffset responseTimeLimit = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(expectedResponseActiveDurationMinutes));
            List<ExpectedResponseDO> staleResponseList = uow.ExpectedResponseRepository.GetQuery()
                .Where(x => x.Status == ExpectedResponseStatus.Active && x.LastUpdated < responseTimeLimit).ToList();
            foreach (var er in staleResponseList)
            {
                _er.MarkAsStale(uow, er);
                LogSuccess("Expected response is stale");
            }
        }

    }
}
