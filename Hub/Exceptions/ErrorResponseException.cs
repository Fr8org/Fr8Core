using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Exceptions
{
    public class ErrorResponseException : Exception
    {
        public ContainerDTO ContainerDTO { get; private set; }

        public ErrorResponseException()
        {
        }

        public ErrorResponseException(ContainerDTO containerDTO)
            : base("")
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
