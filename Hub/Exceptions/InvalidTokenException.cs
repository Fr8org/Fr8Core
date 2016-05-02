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
    /// <summary>
    /// Exception is thrown when auth token cannot be obtained in runtime.
    /// </summary>
    public class InvalidTokenRuntimeException : ActivityExecutionException
    {
        public InvalidTokenRuntimeException(ActivityDTO activityDTO, ContainerDTO containerDTO, string message) :
            base(activityDTO, containerDTO, message)
        {

        }

        public InvalidTokenRuntimeException(ActivityDTO activityDTO, string message) : 
            base(message, null)
        {
            FailedActivityDTO = activityDTO;
        }

        public InvalidTokenRuntimeException(ContainerDTO containerDTO, string message) : 
            base(message, null)
        {
            ContainerDTO = containerDTO;
        }

        public InvalidTokenRuntimeException(ActivityDTO activityDTO) : 
            base()
        {
            FailedActivityDTO = activityDTO;
        }
    }
}
