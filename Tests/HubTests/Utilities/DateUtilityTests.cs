using System;
using Fr8.Infrastructure.Utilities;
using NUnit.Framework;
using Fr8.Testing.Unit;

namespace HubTests.Utilities
{
    [TestFixture]
    public class DateUtilityTests : BaseTest
    {
        [Test]
        public void CalculateDayBucket()
        {
            var date = new DateTime(2016, 04, 26, 05, 21, 36);
            var dayBucket = DateUtility.CalculateDayBucket(date);

            Assert.NotNull(dayBucket);
            Assert.AreEqual(0, dayBucket.Value.Second);
            Assert.AreEqual(0, dayBucket.Value.Minute);
            Assert.AreEqual(0, dayBucket.Value.Hour);
            Assert.AreEqual(26, dayBucket.Value.Day);
            Assert.AreEqual(4, dayBucket.Value.Month);
            Assert.AreEqual(2016, dayBucket.Value.Year);
        }

        [Test]
        public void CalculateWeekBucket()
        {
            var date = new DateTime(2016, 04, 26, 05, 21, 36);
            var dayBucket = DateUtility.CalculateWeekBucket(date);

            Assert.NotNull(dayBucket);
            Assert.AreEqual(0, dayBucket.Value.Second);
            Assert.AreEqual(0, dayBucket.Value.Minute);
            Assert.AreEqual(0, dayBucket.Value.Hour);
            Assert.AreEqual(24, dayBucket.Value.Day);
            Assert.AreEqual(4, dayBucket.Value.Month);
            Assert.AreEqual(2016, dayBucket.Value.Year);
        }

        [Test]
        public void CalculateMonthBucket()
        {
            var date = new DateTime(2016, 04, 26, 05, 21, 36);
            var dayBucket = DateUtility.CalculateMonthBucket(date);

            Assert.NotNull(dayBucket);
            Assert.AreEqual(0, dayBucket.Value.Second);
            Assert.AreEqual(0, dayBucket.Value.Minute);
            Assert.AreEqual(0, dayBucket.Value.Hour);
            Assert.AreEqual(1, dayBucket.Value.Day);
            Assert.AreEqual(4, dayBucket.Value.Month);
            Assert.AreEqual(2016, dayBucket.Value.Year);
        }
        
        [Test]
        public void CalculateYearBucket()
        {
            var date = new DateTime(2016, 04, 26, 05, 21, 36);
            var dayBucket = DateUtility.CalculateYearBucket(date);

            Assert.NotNull(dayBucket);
            Assert.AreEqual(0, dayBucket.Value.Second);
            Assert.AreEqual(0, dayBucket.Value.Minute);
            Assert.AreEqual(0, dayBucket.Value.Hour);
            Assert.AreEqual(1, dayBucket.Value.Day);
            Assert.AreEqual(1, dayBucket.Value.Month);
            Assert.AreEqual(2016, dayBucket.Value.Year);
        }
    }
}
