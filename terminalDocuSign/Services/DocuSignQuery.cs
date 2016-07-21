using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalDocuSign.Services
{
    public class DocuSignQuery
    {
        public static readonly KeyValueDTO[] Statuses =
        {
            new KeyValueDTO("Any status", null), //should be an null value so the DocuSignAPI can include all
            new KeyValueDTO("Sent", "sent"),
            new KeyValueDTO("Delivered", "delivered"),
            new KeyValueDTO("Signed", "signed"),
            new KeyValueDTO("Completed", "completed"),
            new KeyValueDTO("Declined", "declined"),
            new KeyValueDTO("Voided", "voided"),
            new KeyValueDTO("Timed Out", "timedout"),
            new KeyValueDTO("Authoritative Copy", "authoritativecopy"),
            new KeyValueDTO("Transfer Completed", "transfercompleted"),
            new KeyValueDTO("Template", "template"),
            new KeyValueDTO("Correct", "correct"),
            new KeyValueDTO("Created", "created"),
            new KeyValueDTO("Delivered", "delivered"),
            new KeyValueDTO("Signed", "signed"),
            new KeyValueDTO("Declined", "declined"),
            new KeyValueDTO("Completed", "completed"),
            new KeyValueDTO("Fax Pending", "faxpending"),
            new KeyValueDTO("Auto Responded", "autoresponded"),
        };

        public DocuSignQuery()
        {
            Conditions = new List<FilterConditionDTO>();
        }

        public DateTime? FromDate;
        public DateTime? ToDate;

        public string SearchText;
        public string Status;
        public string Folder;

        public List<FilterConditionDTO> Conditions { get; set; }
    }
}