using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalAsana.Asana.Entities
{
    public class AsanaProjectInsertion
    {
        public string Task { get; set; }
        public string Project { get; set; }
        public string InsertAfter { get; set; }
        public string InsertBefore { get; set; }
        public string Section { get; set; }
    }
}