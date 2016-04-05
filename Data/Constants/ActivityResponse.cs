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
        ReProcessChildren,
        ExecuteClientActivity,
        ShowDocumentation,
        JumpToActivity,
        JumpToSubplan,
        RequestLaunch,
        LaunchAdditionalPlan,

        //new op codes
        Jump,
        Call,
        Break,
    }

    public enum PlanType
    {
        Ongoing,
        RunOnce
    }
}
