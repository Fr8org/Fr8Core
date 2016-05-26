using System;
using Fr8Data.Constants;
using Hub.Managers;
using Fr8Data.Managers;

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
