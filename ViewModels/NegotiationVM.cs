using System;
using System.Collections.Generic;
using System.Linq;

namespace Web.ViewModels
{
    public class NegotiationVM
    {
        public NegotiationVM()
        {
            Questions = new List<NegotiationQuestionVM>();
        }

        public int? Id { get; set; }
        public int? BookingRequestID { get; set; }
        public string Name { get; set; }
        public List<NegotiationQuestionVM> Questions { get; set; }
    }

    public class NegotiationQuestionVM
    {
        public NegotiationQuestionVM()
        {
            Answers = new List<NegotiationAnswerVM>();
        }

        public int Id { get; set; }
        public int? CalendarID { get; set; }
        public string Text { get; set; }
        public List<NegotiationAnswerVM> Answers { get; set; }
        public string AnswerType { get; set; }  
    }

    public class NegotiationAnswerVM
    {
        public NegotiationAnswerVM()
        {
            VotedByList = new List<string>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public int? EventID { get; set; }
        public String SuggestedBy { get; set; }

        public DateTimeOffset? EventStartDate { get; set; }
        public DateTimeOffset? EventEndDate { get; set; }
        
        public bool Selected { get; set; }
        public int? AnswerState { get; set; }

        public List<String> VotedByList { get; set; }
        public String VotedBy { get { return VotedByList != null ? String.Join(",", VotedByList.Where(a => !String.IsNullOrEmpty(a)).Select(a => "'" + a + "'")) : ""; } set {  } }
    }
}