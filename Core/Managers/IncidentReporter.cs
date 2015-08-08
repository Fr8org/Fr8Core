using System;
using System.Text;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Core.Services;
using Data.Exceptions;
using StructureMap;
using Utilities.Logging;

namespace Core.Managers
{
    public class IncidentReporter
    {
        public void SubscribeToAlerts()
        {
            EventManager.AlertEmailProcessingFailure += ProcessAlert_EmailProcessingFailure;
            //AlertManager.AlertBookingRequestProcessingTimeout += ProcessBRTimeout;
            //AlertManager.AlertBookingRequestMarkedProcessed += ProcessBRMarkedProcessed;
            EventManager.AlertError_EmailSendFailure += ProcessEmailSendFailure;
            //AlertManager.AlertErrorSyncingCalendar += ProcessErrorSyncingCalendar;
            EventManager.AlertResponseReceived += AlertManagerOnAlertResponseReceived;
            //AlertManager.AlertAttendeeUnresponsivenessThresholdReached += ProcessAttendeeUnresponsivenessThresholdReached;
            //AlertManager.AlertBookingRequestCheckedOut += ProcessBRCheckedOut;
            EventManager.AlertUserRegistrationError += ReportUserRegistrationError;
            //AlertManager.AlertBookingRequestMerged += BookingRequestMerged;
        }

        private void ProcessAttendeeUnresponsivenessThresholdReached(int expectedResponseId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expectedResponseDO = uow.ExpectedResponseRepository.GetByKey(expectedResponseId);
                if (expectedResponseDO == null)
                    throw new EntityNotFoundException<ExpectedResponseDO>(expectedResponseId);
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Negotiation";
                incidentDO.SecondaryCategory = "ClarificationRequest";
                incidentDO.CustomerId = expectedResponseDO.UserID;
                incidentDO.ObjectId = expectedResponseId.ToString();
                incidentDO.Activity = "UnresponsiveAttendee";
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
        }

        private void AlertManagerOnAlertResponseReceived(int bookingRequestId, string userID, string customerID)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "BookingRequest";
                incidentDO.SecondaryCategory = "Response Received";
                incidentDO.CustomerId = customerID;
                incidentDO.BookerId = userID;
                incidentDO.ObjectId = bookingRequestId.ToString();
                incidentDO.Activity = "Response Recieved";
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        public void ProcessAlert_EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Email";
                incidentDO.SecondaryCategory = "Failure";
                incidentDO.Priority = 5;
                incidentDO.Activity = "Intake";
                incidentDO.Data = errorMessage;
                incidentDO.ObjectId = null;
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        //public void ProcessBRTimeout(int bookingRequestId, string bookerId)
        //{

        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
        //        IncidentDO incidentDO = new IncidentDO();
        //        incidentDO.PrimaryCategory = "BookingRequest";
        //        incidentDO.SecondaryCategory = null;
        //        incidentDO.Activity = "Timeout";
        //        incidentDO.ObjectId = bookingRequestId.ToString();
        //        incidentDO.CustomerId = bookingRequestDO.CustomerID;
        //        incidentDO.BookerId = bookingRequestDO.BookerID;
        //        uow.IncidentRepository.Add(incidentDO);
        //        uow.SaveChanges();
        //    }
        //}


        private void ProcessEmailSendFailure(int emailId, string message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Email";
                incidentDO.SecondaryCategory = "Failure";
                incidentDO.Activity = "Send";
                incidentDO.ObjectId = emailId.ToString();
                incidentDO.Data = message;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
            Email _email = ObjectFactory.GetInstance<Email>();
		//_email.SendAlertEmail("Alert! Kwasant Error Reported: EmailSendFailure",
		//			    string.Format(
		//				  "EmailID: {0}\r\n" +
		//				  "Message: {1}",
		//				  emailId, message));
        }
        //private void ProcessErrorSyncingCalendar(IRemoteCalendarAuthDataDO authData, IRemoteCalendarLinkDO calendarLink = null)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        IncidentDO incidentDO = new IncidentDO();
        //        incidentDO.PrimaryCategory = "Calendar";
        //        incidentDO.SecondaryCategory = "Failure";
        //        incidentDO.Activity = "Synchronization";
        //        incidentDO.ObjectId = authData.Id.ToString();
        //        incidentDO.CustomerId = authData.UserID;
        //        if (calendarLink != null)
        //        {
        //            incidentDO.Data = string.Format("Link #{0}: {1}", calendarLink.Id, calendarLink.LastSynchronizationResult);
        //        }
        //        uow.IncidentRepository.Add(incidentDO);
        //        uow.SaveChanges();
        //    }

        //    var emailBodyBuilder = new StringBuilder();
        //    emailBodyBuilder.AppendFormat("CalendarSync failure for calendar auth data #{0} ({1}):\r\n", authData.Id,
        //                                  authData.Provider.Name);
        //    emailBodyBuilder.AppendFormat("Customer id: {0}\r\n", authData.UserID);
        //    if (calendarLink != null)
        //    {
        //        emailBodyBuilder.AppendFormat("Calendar link id: {0}\r\n", calendarLink.Id);
        //        emailBodyBuilder.AppendFormat("Local calendar id: {0}\r\n", calendarLink.LocalCalendarID);
        //        emailBodyBuilder.AppendFormat("Remote calendar url: {0}\r\n", calendarLink.RemoteCalendarHref);
        //        emailBodyBuilder.AppendFormat("{0}\r\n", calendarLink.LastSynchronizationResult);
        //    }

        //    Email email = ObjectFactory.GetInstance<Email>();
        //   // email.SendAlertEmail("CalendarSync failure", emailBodyBuilder.ToString());
        //}

        //public void ProcessSubmittedNote(int bookingRequestId, string note)
        //{
        //    if (String.IsNullOrEmpty(note))
        //        throw new ArgumentException("Empty note.", "note");
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var curBookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
        //        if (curBookingRequest == null)
        //            throw new EntityNotFoundException<BookingRequestDO>(bookingRequestId);
        //        var incidentDO = new IncidentDO
        //            {
        //                PrimaryCategory = "BookingRequest",
        //                SecondaryCategory = "Note",
        //                Activity = "Created",
        //                BookerId = curBookingRequest.BookerID,
        //                ObjectId = bookingRequestId.ToString(),
        //                Data = note
        //            };
        //        uow.IncidentRepository.Add(incidentDO);
        //        uow.SaveChanges();
        //    }
        //}

        //public void ProcessBRCheckedOut(int bookingRequestId, string bookerId)
        //{
        //    //BookingRequest _br = new BookingRequest();
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
        //        if (bookingRequestDO == null)
        //            throw new ArgumentException(string.Format("Cannot find a Booking Request by given id:{0}", bookingRequestId), "bookingRequestId");
        //        string status = bookingRequestDO.BookingRequestStateTemplate.Name;
        //        IncidentDO curAction = new IncidentDO()
        //        {
        //            PrimaryCategory = "BookingRequest",
        //            SecondaryCategory = null,
        //            Activity = "Checkout",
        //            CustomerId = bookingRequestDO.Customer.Id,
        //            ObjectId = bookingRequestId.ToString(),
        //            BookerId = bookerId,
        //        };

        //       // int getMinutinQueue = _br.GetTimeInQueue(uow, bookingRequestDO.Id.ToString());

        //        //curAction.Data = string.Format("Time To Process: {0}", getMinutinQueue);

        //        //uow.IncidentRepository.Add(curAction);
        //        uow.SaveChanges();
        //    }
        //}

        //private void ProcessBRMarkedProcessed(int bookingRequestId, string bookerId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
        //        if (bookingRequestDO == null)
        //            throw new ArgumentException(string.Format("Cannot find a Booking Request by given id:{0}", bookingRequestId), "bookingRequestId");
        //        IncidentDO curAction = new IncidentDO()
        //        {
        //            PrimaryCategory = "BookingRequest",
        //            SecondaryCategory = "BookerAction",
        //            Activity = "MarkedAsProcessed",
        //            CustomerId = bookingRequestDO.CustomerID,
        //            ObjectId = bookingRequestId.ToString(),
        //            BookerId = bookerId,
        //        };

        //       // var br = ObjectFactory.GetInstance<BookingRequest>();
        //       // int getMinutinQueue = br.GetTimeInQueue(uow, bookingRequestDO.Id.ToString());

        //       // curAction.Data = string.Format("Time To Process: {0}", getMinutinQueue);
        //        uow.IncidentRepository.Add(curAction);
        //        uow.SaveChanges();
        //    }
        //}

        public void ReportUserRegistrationError(Exception ex)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "DockYardAccount";
                incidentDO.SecondaryCategory = "Error";
                incidentDO.Activity = "Registration";
                incidentDO.Data = ex.Message;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();

                string logData = string.Format("{0} {1} {2}:" + " ObjectId: {3} CustomerId: {4}",
                        incidentDO.PrimaryCategory,
                        incidentDO.SecondaryCategory,
                        incidentDO.Activity,
                        incidentDO.ObjectId,
                        incidentDO.CustomerId);

                Logger.GetLogger().Info(logData);
            }
        }



        public void BookingRequestMerged(int originalBRId, int targetBRId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "BookingRequest";
                incidentDO.SecondaryCategory = "BookerAction";
                incidentDO.Activity = "MergedBRs";
                incidentDO.ObjectId = originalBRId.ToString();

                string logData = string.Format("{0} {1} {2}: ",
                        incidentDO.PrimaryCategory,
                        incidentDO.SecondaryCategory,
                        incidentDO.Activity);

                incidentDO.Data = logData + incidentDO.ObjectId;
                uow.IncidentRepository.Add(incidentDO);
                Logger.GetLogger().Info(incidentDO.Data);
                uow.SaveChanges();

                incidentDO.ObjectId = targetBRId.ToString();
                incidentDO.Data = logData + incidentDO.ObjectId;
                uow.IncidentRepository.Add(incidentDO);
                Logger.GetLogger().Info(incidentDO.Data);
                uow.SaveChanges();
            }
        }
    }


}
