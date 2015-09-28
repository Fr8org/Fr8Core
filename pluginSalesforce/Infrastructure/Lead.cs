using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pluginSalesforce.Infrastructure
{
    public class Lead
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
    }
}