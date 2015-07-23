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

        public void CheckUserHasCalendar(DockyardAccountDO curDockyardAccount)
        {
            if (curDockyardAccount == null)
                throw new ArgumentNullException("curDockyardAccount");

            if (curDockyardAccount.Calendars.Count() == 0)
            {
                var curCalendar = new CalendarDO
                {
                    Name = "Default Calendar",
                    Owner = curDockyardAccount,
                    OwnerID = curDockyardAccount.Id
                };
                curDockyardAccount.Calendars.Add(curCalendar);
                Add(curCalendar);
            }
        }
    }


    public interface ICalendarRepository : IGenericRepository<CalendarDO>
    {

    }
}