using System;
using Data.Constants;
using Hub.Managers;

namespace HubTests.Services.Container
{
    class CallerActivityMock : ActivityMockBase
    {
        private readonly Guid _jumopTo;

        public CallerActivityMock(ICrateManager crateManager, Guid jumopTo) 
            : base(crateManager)
        {
            _jumopTo = jumopTo;
        }

        protected override void Run(Guid id, ActivityExecutionMode executionMode)
        {
            RequestCall(_jumopTo);
        }
    }
}
