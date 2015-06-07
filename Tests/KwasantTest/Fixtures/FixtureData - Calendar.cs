using Data.Entities;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public CalendarDO TestCalendar1()
        {
            var calendarDO = new CalendarDO
            {
                Name = "Default"
            };
            return calendarDO;
        }
    }
}
