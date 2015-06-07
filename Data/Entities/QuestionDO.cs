using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{

    public class QuestionDO : BaseDO, IQuestionDO, IDeleteHook
    {
        public QuestionDO()
        {
            Answers = new List<AnswerDO>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }
        public string AnswerType { get; set; }
        public string Response { get; set; }

        [ForeignKey("QuestionStatusTemplate")]
        public int? QuestionStatus { get; set; }
        public _QuestionStatusTemplate QuestionStatusTemplate { get; set; }

        [ForeignKey("Calendar")]
        public int? CalendarID { get; set; }
        public virtual CalendarDO Calendar { get; set; }

        [ForeignKey("Negotiation"), Required]
        public int? NegotiationId { get; set; }
        public virtual NegotiationDO Negotiation { get; set; }

        [InverseProperty("Question")]
        public virtual List<AnswerDO> Answers { get; set; }


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
            if (Negotiation != null)
            {
                var br = Negotiation.BookingRequest;
                if (br != null)
                    br.LastUpdated = DateTime.Now;
            }
        }
    }
}
