using System;

namespace Fr8Infrastructure.Communication
{
    public class RestfulServiceException : ApplicationException
    {
        public int StatusCode
        {
            get; 
            private set;
        }

        public RestfulServiceException(int statusCode)
        {
            StatusCode = statusCode;
        }

        public RestfulServiceException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;            
        }

        public RestfulServiceException(int statusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
