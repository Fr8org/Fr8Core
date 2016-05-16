using System;
using Fr8Data.Constants;
using Hub.Managers;
using NUnit.Framework;

namespace HubTests.Services.Container
{
    class SuspenderActivityMock : ActivityMockBase
    {
        private bool _firstCall = true;

        public SuspenderActivityMock(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        protected override void Run(Guid id, ActivityExecutionMode executionMode)
        {
            if (_firstCall)
            {
                _firstCall = false;
                OperationalState.CallStack.StoreLocalData("Suspend", "data");
                RequestHubExecutionSuspension("Hey, wait for me!");
            }
            else
            {
                Assert.AreEqual("data", OperationalState.CallStack.GetLocalData<string>("Suspend"), "Local data is missing");
            }
        }
    }
}
