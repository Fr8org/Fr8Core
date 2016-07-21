namespace Fr8.Infrastructure.Data.Constants
{
    public enum ActivityResponse
    {
        Null = 0,
        Success = 1,
        Error = 2,
        RequestTerminate  =3,
        RequestSuspend = 4,
        SkipChildren = 5,
        ExecuteClientActivity = 7,
        JumpToActivity = 9,
        JumpToSubplan = 10,
        LaunchAdditionalPlan = 11,

        //new op codes
        CallAndReturn = 12,
        Break = 13,
    }

    public enum PlanType
    {
        Monitoring,
        RunOnce
    }
}
