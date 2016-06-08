using System;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public enum ErrorType
    {
        Generic,
        Authentication,
        Critical
    }

    public class ResponseMessageDTO
    {
        public ResponseMessageDTO() { }

        public ResponseMessageDTO(string type)
        {
            Type = type;
        }

        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("type")]
        public string Type { get; protected set; }
        [JsonProperty("details")]
        public object Details { get; set; }
        [JsonProperty("currentActivity")]
        public string CurrentActivity { get; set; }
        [JsonProperty("currentTerminal")]
        public string CurrentTerminal { get; set; }

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

        public static ResponseMessageDTO Create(string message, ErrorType errorType, string errorCode, object details)
        {
            return new ResponseMessageDTO(ErrorTypeToString(errorType))
            {
                Details = details,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public class ErrorDTO : ResponseMessageDTO
    {
        protected ErrorDTO() {  }

        protected ErrorDTO(string type)
        {
            Type = type;
        }

        public static ErrorDTO InternalError(string message, string errorCode = null, object details = null, string activity = null, string terminal = null)
        {
            return Create(message, ErrorType.Generic, errorCode, details, activity, terminal);
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
            return Create(message, ErrorType.Authentication, errorCode, details, null, null);
        }

        public static ErrorDTO CriticalError(string message, string errorCode = null, object details = null)
        {
            return Create(message, ErrorType.Critical, errorCode, details, null, null);
        }

        public static ErrorDTO Create(string message, ErrorType errorType, string errorCode, object details, string activity, string terminal)
        {
            return new ErrorDTO (ErrorTypeToString(errorType))
            {
                Details = details,
                Message = message,
                ErrorCode = errorCode,
                CurrentActivity = activity,
                CurrentTerminal = terminal
            };
        }
    }


}
