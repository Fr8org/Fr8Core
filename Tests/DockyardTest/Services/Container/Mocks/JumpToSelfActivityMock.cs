using System;
using Data.Constants;
using Hub.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DockyardTest.Services.Container
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
                OperationalState.StoreLocalData("Jump", "data");
                RequestJumpToActivity(id);
            }
            else
            {
                Assert.AreEqual("data", OperationalState.GetLocalData<string>("Jump"), "Local data is missing");
            }
        }
    }
}
