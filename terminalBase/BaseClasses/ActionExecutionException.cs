using System;
using Data.Constants;

namespace TerminalBase.BaseClasses
{
    public class ActionExecutionException : Exception
    {
        public ActivityErrorCode? ErrorCode
        {
            get;
        }

        public ActionExecutionException(string message, ActivityErrorCode? errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ActionExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ActionExecutionException()
        {
        }
    }
}