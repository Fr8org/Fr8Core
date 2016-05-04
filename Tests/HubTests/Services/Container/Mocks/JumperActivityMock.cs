using System;
using Fr8Data.Constants;
using Hub.Managers;

namespace HubTests.Services.Container
{
    class JumperActivityMock : ActivityMockBase
    {
        private readonly Guid _jumopTo;

        public JumperActivityMock(ICrateManager crateManager, Guid jumopTo) 
            : base(crateManager)
        {
            _jumopTo = jumopTo;
        }

        protected override void Run(Guid id, ActivityExecutionMode executionMode)
        {
            RequestJumpToActivity(_jumopTo);
        }
    }
}
