using System;

namespace Fr8.Infrastructure.Utilities
{
    public static class ExceptionExtesion
    {
        public static string GetFullExceptionMessage(this Exception e, string msgs = "")
        {
            if (e == null) return string.Empty;
            if (msgs == "") msgs = e.Message;
            if (e.InnerException != null)
                msgs += "\r\nInnerException: " + GetFullExceptionMessage(e.InnerException);
            return msgs;
        }
    }
}
