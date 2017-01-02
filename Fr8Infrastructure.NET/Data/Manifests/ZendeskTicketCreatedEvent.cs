using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class ZendeskTicketCreatedEvent : Manifest
    {
        public string Assignee { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string UserId { get; set; }
        public string TicketId { get; set; }

        public ZendeskTicketCreatedEvent() : base(MT.ZendeskTicketEvent)
        {

        }
    }
}
