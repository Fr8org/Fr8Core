using System;
using Data.Constants;
using Hub.Managers;

namespace HubTests.Services.Container
{
    class SubplanJumperActivityMock : ActivityMockBase
    {
        private readonly Guid _jumopTo;

        public SubplanJumperActivityMock(ICrateManager crateManager, Guid jumopTo) 
            : base(crateManager)
        {
            _jumopTo = jumopTo;
        }

        protected override void Run(Guid id, ActivityExecutionMode executionMode)
        {
            RequestJumpToSubplan(_jumopTo);
        }
    }
}
