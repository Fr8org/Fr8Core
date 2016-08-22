using System;
using System.Net;
using Fr8.Infrastructure.Utilities;

namespace Fr8.TerminalBase.Errors
{
    public class TerminalCodedException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public TerminalErrorCode ErrorCode { get; private set; }
        private string Details { get; set; }

        public override string Message {
            get {
                return (!string.IsNullOrEmpty(Details) ? string.Format("{0}: {1}", base.Message, Details) : base.Message);
            }
        }

        public TerminalCodedException(TerminalErrorCode errorCode, string details = null) : base(errorCode.GetEnumDescription()) {
            ErrorCode = errorCode;
            StatusCode = HttpStatusCode.InternalServerError;
            Details = details;
        }
    }
}
