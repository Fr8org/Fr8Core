using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Constants
{
    public enum ActionResponse
    {
        Null = 0,
        Success,
        Error,
        RequestTerminate,
        RequestSuspend
    }
}
