using System;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HubTests.Services.Container
{
    class JumpToSelfActivityMock : ActivityMockBase
    {
        private int _index;

        public JumpToSelfActivityMock(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        protected override void Run(Guid id, ActivityExecutionMode executionMode)
        {
            _index++;

            if (_index <= 1)
            {
                OperationalState.CallStack.StoreLocalData("Jump", "data");
                RequestJumpToActivity(id);
            }
            else
            {
                Assert.AreEqual("data", OperationalState.CallStack.GetLocalData<string>("Jump"), "Local data is missing");
            }
        }
    }
}
