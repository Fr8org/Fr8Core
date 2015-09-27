using System;
using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.States;

using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    
    [TestFixture]

    public class ProcessExecuteTests: BaseTest
    {
        private IProcess _process;

        [SetUp]
        //constructor method as it is run at the test start
        public override void SetUp()
        {
            base.SetUp();

            _process = ObjectFactory.GetInstance<IProcess>();
        }

        [Test]

        //NUnit framework feature: ability to test for an exception
        [ExpectedException(typeof(ArgumentNullException))]

        public void ExecuteFailsIfNullProcessDo()
        {
            //null is passed instead of ProcessDo object
            _process.Execute(null);
        
        }
        
        [Test]

        //NUnit framework feature: ability to test for an exception
        [ExpectedException(typeof(ArgumentNullException))]

        public void ExecuteFailsIfNullProcessDoCurrentActivity()
        {
            //Get ProcessDO entity from static partial class FixtureData for already prepared data
            //The CurrentActivity value is already set to null and pass it immediately to service
            _process.Execute(FixtureData.TestProcessCurrentActivityNULL());

        }
    }

}

