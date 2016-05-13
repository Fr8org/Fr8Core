namespace Fr8Data.Constants
{
    public enum ActivityResponse
    {
        Null = 0,
        Success,
        Error,
        RequestTerminate,
        RequestSuspend,
        SkipChildren,
        ReprocessChildren,
        ExecuteClientActivity,
        ShowDocumentation,
        JumpToActivity,
        JumpToSubplan,
        LaunchAdditionalPlan,

        //new op codes
        CallAndReturn,
        Break,
    }

    public enum PlanType
    {
        Monitoring,
        RunOnce
    }
}
