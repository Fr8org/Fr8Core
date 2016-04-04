using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Constants
{
    public enum ActivityResponse
    {
        Null = 0,
        Success,
        Error,
        RequestTerminate,
        RequestSuspend,
        SkipChildren,
        ExecuteClientActivity,
        ShowDocumentation,
        JumpToActivity,
        JumpToSubplan,
        RequestLaunch,

        //new op codes
        Jump,
        JumpFar,
        Call,
        Break,
    }

    public enum PlanType
    {
        Ongoing,
        RunOnce
    }
}
