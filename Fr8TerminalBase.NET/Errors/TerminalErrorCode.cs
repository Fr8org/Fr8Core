using System.ComponentModel;

namespace Fr8.TerminalBase.Errors
{
    //TODO remove this?
    public enum TerminalErrorCode
    {
        [Description("The terminal was passed a request that required a connection string, and it was not found.")]
        SQL_SERVER_CONNECTION_STRING_MISSING = 10000,
        [Description("The terminal was unable to connect with the provided database connection string.")]
        SQL_SERVER_CONNECTION_FAILED,
        PAYLOAD_DATA_MISSING,
        PAYLOAD_DATA_INVALID
    }
}
