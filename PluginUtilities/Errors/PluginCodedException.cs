using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace PluginUtilities {
    public class PluginCodedException : Exception {
        public HttpStatusCode StatusCode { get; private set; }
        public PluginErrorCode ErrorCode { get; private set; }
        private string Details { get; set; }

        public override string Message {
            get {
                return (!string.IsNullOrEmpty(Details) ? string.Format("{0}: {1}", base.Message, Details) : base.Message);
            }
        }

        public PluginCodedException(PluginErrorCode errorCode, string details = null) : base(errorCode.GetEnumDescription()) {
            ErrorCode = errorCode;
            StatusCode = HttpStatusCode.InternalServerError;
            Details = details;
        }
    }
}
