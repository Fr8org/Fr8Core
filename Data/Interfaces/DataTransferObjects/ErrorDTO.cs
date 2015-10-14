using System;

namespace Data.Interfaces.DataTransferObjects
{
    public enum ErrorType
    {
        Generic,
        Authentication,
        Critical
    }

    public class ErrorDTO
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string Type { get; protected set; }
        public object Details { get; set; }

        protected ErrorDTO(string type)
        {
            Type = type;
        }
        
        protected static string ErrorTypeToString(ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.Generic:
                    return "int";

                case ErrorType.Authentication:
                    return "auth";

                case ErrorType.Critical:
                    return "critical";

                default:
                    throw new ArgumentOutOfRangeException("errorType");
            }
        }

        public static ErrorDTO InternalError(string message, string errorCode = null, object details = null)
        {
            return Create(message, ErrorType.Generic, errorCode, details);
        }


        public static ErrorDTO AuthenticationError()
        {
            return new ErrorDTO(ErrorTypeToString(ErrorType.Authentication));
        }

        public static ErrorDTO InternalError()
        {
            return new ErrorDTO(ErrorTypeToString(ErrorType.Generic));
        }

        public static ErrorDTO AuthenticationError(string message, string errorCode = null, object details = null)
        {
            return Create(message, ErrorType.Authentication, errorCode, details);
        }

        public static ErrorDTO CriticalError(string message, string errorCode = null, object details = null)
        {
            return Create(message, ErrorType.Critical, errorCode, details);
        }

        public static ErrorDTO Create(string message, ErrorType errorType, string errorCode, object details)
        {
            return new ErrorDTO (ErrorTypeToString(errorType))
            {
                Details = details,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }


}
