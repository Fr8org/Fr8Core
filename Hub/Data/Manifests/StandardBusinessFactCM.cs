using System;

namespace Fr8Data.Manifests
{
    public class StandardBusinessFactCM : Manifest
    {
        public StandardBusinessFactCM()
            : base(Constants.MT.StandardBusinessFact)
        {
        }

        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Activity { get; set; }
        public string Status { get; set; }
        public string ObjectId { get; set; }
        public string CustomerId { get; set; }
        public string OwnerId { get; set; }
        public string Data { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? DayBucket { get; set; }
        public DateTime? WeekBucket { get; set; }
        public DateTime? MonthBucket { get; set; }
        public DateTime? YearBucket { get; set; }
        public string UserType { get; set; }
    }
}
