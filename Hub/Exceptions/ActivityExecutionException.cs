using Data.Entities;
using Data.Interfaces.DataTransferObjects;
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
        public ActivityDTO FailedActivityDTO { get; protected set; }
        public ContainerDTO ContainerDTO { get; protected set; }

        public string ErrorMessage
        {
            get
            {
                return GetErrorMessage();
            }
        }

        public ActivityExecutionException()
        {
        }

        public ActivityExecutionException(ActivityDTO activityDTO, ContainerDTO containerDTO, string message) : base(message)
        {
            FailedActivityDTO = activityDTO;
            ContainerDTO = containerDTO;

        }

        public ActivityExecutionException(string message, Exception innerException)
            : base(message ?? string.Empty, innerException)
        {
           
        }

        public ActivityExecutionException(ContainerDTO containerDTO, ActivityDTO activityDTO, string message, Exception innerException)
            : base(string.IsNullOrEmpty(message) ? innerException.Message: message, innerException)
        {
            ContainerDTO = containerDTO;
            FailedActivityDTO = activityDTO;
        }

        protected ActivityExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected virtual string GetErrorMessage()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                return String.Format("Failed to run activity \"{0}\". {1}", FailedActivityDTO?.Label, Message);
            }
            else
            {
                return String.Format("Failed to run activity \"{0}\". Please, make sure it is set up correctly.", FailedActivityDTO?.Label);
            }            
        }
    }
}
