using System;
using System.Collections.Generic;
using Data.Interfaces;

namespace terminalUtilities.SendGrid
{
    public class MailerDO : IMailerDO
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
        public IEmailDO Email { get; set; }
        public string Footer { get; set; }
    }
}