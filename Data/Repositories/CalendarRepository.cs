using System;
using System.Linq;
using Data.Interfaces;
using Data.Entities;
using Data.Validations;
using FluentValidation;

namespace Data.Repositories
{
    public class CalendarRepository : GenericRepository<CalendarDO>, ICalendarRepository
    {
        private readonly CalendarValidator _curValidator;
        internal CalendarRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new CalendarValidator();
        }

        public new void Add(CalendarDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }

        public void CheckUserHasCalendar(UserDO curUser)
        {
            if (curUser == null)
                throw new ArgumentNullException("curUser");

            if (curUser.Calendars.Count() == 0)
            {
                var curCalendar = new CalendarDO
                {
                    Name = "Default Calendar",
                    Owner = curUser,
                    OwnerID = curUser.Id
                };
                curUser.Calendars.Add(curCalendar);
                Add(curCalendar);
            }
        }
    }


    public interface ICalendarRepository : IGenericRepository<CalendarDO>
    {

    }
}