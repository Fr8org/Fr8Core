using System;
using System.ComponentModel.DataAnnotations.Schema;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;

namespace Data.Entities
{
    public class FactDO : HistoryItemDO
    {
        public FactDO()
        {
            var t = 0;//For debug breakpoint purposes
        }

        [ForeignKey("CreatedBy")]
        public string CreatedByID { get; set; }
        public virtual Fr8AccountDO CreatedBy { get; set; }

        public StandardBusinessFactCM ToFactCM()
        {
            var createDate = DateTime.Now;

            return new StandardBusinessFactCM()
            {
                PrimaryCategory = this.PrimaryCategory,
                SecondaryCategory = this.SecondaryCategory,
                Activity = this.Activity,
                Status = this.Status,
                ObjectId = this.ObjectId,
                CustomerId = this.Fr8UserId,
                OwnerId = this.CreatedByID,
                Data = this.Data,

                CreateDate = createDate,
                DayBucket = DateUtility.CalculateDayBucket(createDate),
                WeekBucket = DateUtility.CalculateWeekBucket(createDate),
                MonthBucket = DateUtility.CalculateMonthBucket(createDate),
                YearBucket = DateUtility.CalculateYearBucket(createDate),
                UserType = "Standard"
            };
        }
    }
}
