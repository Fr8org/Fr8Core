using System;

namespace Hub.Managers.APIManagers.Transmitters.Restful
{
    public class RestfulServiceException : ApplicationException
    {
        public int StatusCode
        {
            get; 
            private set;
        }

        public string UserErrorMessage
        {
            get;
            private set;
        }

        public RestfulServiceException(int statusCode)
        {
            StatusCode = statusCode;
        }

        public RestfulServiceException(int statusCode, string message, string userErrorMessage)
            : base(message)
        {
            StatusCode = statusCode;
            UserErrorMessage = userErrorMessage;
        }

        public RestfulServiceException(int statusCode, string message, string userErrorMessage, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            UserErrorMessage = userErrorMessage;
        }
    }
}
