using System;
using Data.Constants;

namespace HubTests.Services.Container
{
    public class ActivityExecutionCall
    {
        public readonly ActivityExecutionMode Mode;
        public readonly Guid Id;

        public ActivityExecutionCall(ActivityExecutionMode mode, Guid id)
        {
            Mode = mode;
            Id = id;
        }
    }
}