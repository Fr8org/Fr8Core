using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class ExecutionPausedException : Exception
    {
        private const string message = "Execution is paused";

        public ExecutionPausedException()
            : base(message)
        {
        }
    }
}
