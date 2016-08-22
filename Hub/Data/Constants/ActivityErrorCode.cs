using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.Constants
{
    public enum ActivityErrorCode
    {
        [Description("The terminal was passed a request that required a connection string, and it was not found.")]
        SQL_SERVER_CONNECTION_STRING_MISSING = 10000,
        [Description("The terminal was unable to connect with the provided database connection string.")]
        SQL_SERVER_CONNECTION_FAILED,
        PAYLOAD_DATA_MISSING,
        PAYLOAD_DATA_INVALID,
        AUTH_TOKEN_NOT_PROVIDED_OR_INVALID,
        DESIGN_TIME_DATA_MISSING
    }
}
