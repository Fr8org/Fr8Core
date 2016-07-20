using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalUtilities.Models
{
    public class TerminalMailerDO
    {
        public int Id { get; set; }

        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        
        public string Handler { get; set; }
        
        public string TemplateName { get; set; }
        
        public IDictionary<string, object> MergeData => null;
        public EmailDTO Email { get; set; }
        public string Footer { get; set; }
    }
}