using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitor.Utility
{
    public static class Utilities
    {
        public static string NormalizeSchema(string endpoint)
        {
            if (endpoint.StartsWith("http://"))
            {
                return endpoint;
            }
            else if (endpoint.StartsWith("https://"))
            {
                return endpoint.Replace("https://", "http://");
            }
            else
            {
                return "http://" + endpoint;
            }
        }
    }
}
