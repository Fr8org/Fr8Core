using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public class SolutionDTO
    {
        public bool IsDeleted { get; set; }
        public bool IsHtml { get; set; }
        public bool IsOutOfDate { get; set; }
        public bool IsPublished { get; set; }
        public bool IsPublishedInPublicKb { get; set; }
        public bool IsReviewed { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string OwnerId { get; set; }
        public string ParentId { get; set; }
        public string RecordTypeId { get; set; }
        public string SolutionLanguage { get; set; }
        public string SolutionName { get; set; }
        public string SolutionNote { get; set; }
        public string SolutionNumber { get; set; }
        public string Status { get; set; }
        public int TimesUsed { get; set; }
    }
}