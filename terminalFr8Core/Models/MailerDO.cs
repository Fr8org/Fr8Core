using System;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace terminalFr8Core.Models
{
    public class TerminalMailerDO
    {
        public int Id { get; set; }

        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        
        public string Handler { get; set; }
        
        public string TemplateName { get; set; }
        
        public IDictionary<string, object> MergeData
        {
            get { return null; }
        }
        public EmailDTO Email { get; set; }
        public string Footer { get; set; }
    }
}