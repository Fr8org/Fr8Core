using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public class DocumentDTO
    {
        public string AuthorId { get; set; }
        public string Body { get; set; }
        public int BodyLength { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }
        public string DeveloperName { get; set; }
        public string FolderId { get; set; }
        public bool IsBodySearchable { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsInternalUseOnly { get; set; }
        public bool IsPublic { get; set; }
        public string Keywords { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string Name { get; set; }
        public string NamespacePrefix { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }
}