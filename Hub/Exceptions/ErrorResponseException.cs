using System;
using System.Runtime.Serialization;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Exceptions
{
    public class ErrorResponseException : Exception
    {
        public ContainerDTO ContainerDTO { get; private set; }

        public ErrorResponseException()
        {
        }

        public ErrorResponseException(ContainerDTO containerDTO, string errorMessage = "")
            : base(errorMessage ?? string.Empty)
        {
            ContainerDTO = containerDTO;
        }

        public ErrorResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ErrorResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
