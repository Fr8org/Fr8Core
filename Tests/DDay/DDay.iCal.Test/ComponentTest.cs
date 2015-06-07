using KwasantICS.DDay.iCal;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class ComponentTest
    {
        [Test, Category("DDay")] //Category(("Component")]
        public void UniqueComponent1()
        {
            iCalendar iCal = new iCalendar();
            DDayEvent evt = iCal.Create<DDayEvent>();

            Assert.IsNotNull(evt.UID);
            Assert.IsNull(evt.Created); // We don't want this to be set automatically
            Assert.IsNotNull(evt.DTStamp);
        }
    }
}
