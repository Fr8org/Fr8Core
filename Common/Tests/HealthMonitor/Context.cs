using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitor
{
    public class Context
    {
        public string InstrumentationKey { get; set; }
        public string ConnectionString { get; set; }
        public Dictionary<string, object> AllArguments { get; set; }
    }
}
