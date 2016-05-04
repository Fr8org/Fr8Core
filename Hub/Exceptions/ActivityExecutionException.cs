using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;

namespace Hub.Exceptions
{
    public class ActivityExecutionException : Exception
    {
        public ActivityDTO FailedActivityDTO { get; private set; }
        public ContainerDTO ContainerDTO { get; private set; }

        private string message;

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

        public ActivityExecutionException(string errorMessage, Exception innerException)
            : base(errorMessage ?? string.Empty, innerException)
        {
           
        }

        public ActivityExecutionException(ContainerDTO containerDTO, ActivityDTO activityDTO, string errorMessage, Exception innerException)
            : base(string.IsNullOrEmpty(errorMessage) ? innerException.Message: errorMessage, innerException)
        {
            ContainerDTO = containerDTO;
            FailedActivityDTO = activityDTO;
            message = errorMessage;
        }

        protected ActivityExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private string GetErrorMessage()
        {
            if (!string.IsNullOrEmpty(message))
            {
                return String.Format("Failed to run activity \"{0}\". {1}", FailedActivityDTO.Label, message);
            }
            else
            {
                return String.Format("Failed to run activity \"{0}\". Please, make sure it is set up correctly.", FailedActivityDTO.Label);
            }            
        }
    }
}
