using Fr8.Infrastructure.Data.DataTransferObjects;

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
            base(null, null)
        {
            FailedActivityDTO = activityDTO;
        }
    }
}
