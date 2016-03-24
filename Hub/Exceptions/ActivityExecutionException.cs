using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class ActivityExecutionException : Exception
    {
        public ActivityDO FailedActivity { get; private set; }

        public ActivityExecutionException()
        {
        }

        public ActivityExecutionException(ActivityDO failedActivity, string errorMessage = "")
            : base(errorMessage ?? string.Empty)
        {
            FailedActivity = failedActivity;
        }

        public ActivityExecutionException(ActivityDO failedActivity, string message, Exception innerException)
            : base(message, innerException)
        {
            FailedActivity = failedActivity;
        }

        protected ActivityExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
