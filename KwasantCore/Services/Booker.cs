using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using StructureMap;

namespace KwasantCore.Services
{
    public class Booker
    {
        public string ChangeBooker(IUnitOfWork uow, int bookingRequestId, string currBooker)
        {
            string result = "";
            try
            {
                BookingRequestDO bookingRequestDO;
                if (bookingRequestId != null)
                {
                    bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                    bookingRequestDO.BookerID = currBooker;
                    uow.SaveChanges();
                    AlertManager.BookingRequestBookerChange(bookingRequestDO.Id, currBooker);
                    result = "Booking request ownership changed successfully!";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string GetName(IUnitOfWork uow, string id)
        {
            string bookerName = string.Empty;

            UserDO userDO = uow.UserRepository.GetByKey(id);

            if (userDO.FirstName != null)
                return bookerName = userDO.FirstName;
            else
                return bookerName = userDO.EmailAddress.Address;

        }
    }
}
