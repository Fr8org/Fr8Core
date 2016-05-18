using System.Collections.Generic;
using HubTests.Services.Container;
using Hub.Managers;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Fr8Data.Managers;

namespace HubTests.Services.Container
{
    public class ContainerExecutionTestBase : BaseTest
    {
        protected ActivityServiceMock ActivityService;
        protected Hub.Interfaces.IContainer Container;
        protected ICrateManager CrateManager;
        protected Hub.Interfaces.IPlan Plan;

        [SetUp]
        //constructor method as it is run at the test start
        public override void SetUp()
        {
            base.SetUp();

            CrateManager = ObjectFactory.GetInstance<ICrateManager>();
            ActivityService = new ActivityServiceMock(ObjectFactory.GetInstance<Hub.Interfaces.IActivity>());
            ObjectFactory.Container.Inject(typeof(Hub.Interfaces.IActivity), ActivityService);
            Container = ObjectFactory.GetInstance<Hub.Interfaces.IContainer>();
            Plan = ObjectFactory.GetInstance<Hub.Interfaces.IPlan>();

            FixtureData.AddTestActivityTemplate();
        }


        protected void AssertExecutionSequence(ActivityExecutionCall[] expected, List<ActivityExecutionCall> actual)
        {
            Assert.AreEqual(expected.Length, actual.Count, "Invalid count of activity executions");

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id, $"Invalid activtiy is executed at step {i}");
                Assert.AreEqual(expected[i].Mode, actual[i].Mode, $"Invalid activtiy execution mode at step {i}");
            }
        }
    }
}
