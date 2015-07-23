using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class AnswerDO : BaseDO, IDeleteHook
    {
        [Key]
        public int Id { get; set; }

        public string Text { get; set; }

        [ForeignKey("Question")]
        public int? QuestionID { get; set; }
        public virtual QuestionDO Question { get; set; }

        [ForeignKey("Event")]
        public int? EventID { get; set; }
        public virtual EventDO Event { get; set; }

        [ForeignKey("AnswerStatusTemplate")]
        public int? AnswerStatus { get; set; }
        public _AnswerStatusTemplate AnswerStatusTemplate { get; set; }

        [ForeignKey("UserDO")]
        public string UserID { get; set; }
        public virtual DockyardAccountDO UserDO { get; set; }

        public override void BeforeSave()
        {
            base.BeforeSave();
            SetBookingRequestLastUpdated();
        }
        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            base.OnModify(originalValues, currentValues);
            SetBookingRequestLastUpdated();
        }

        public void OnDelete(DbPropertyValues originalValues)
        {
            SetBookingRequestLastUpdated();
        }

        private void SetBookingRequestLastUpdated()
        {
            if (Question != null && Question.Negotiation != null)
            {
                var br = Question.Negotiation.BookingRequest;
                if (br != null)
                    br.LastUpdated = DateTime.Now;
            }
        }
    }
}
