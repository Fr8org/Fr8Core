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
        [ExpectedException(typeof(ArgumentNullException))]
        public async void ExecuteFailsIfNullProcessDoCurrentActivity()
        {
            //Get ProcessDO entity from static partial class FixtureData for already prepared data
            //The CurrentActivity value is already set to null and pass it immediately to service
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _process.Execute(uow, FixtureData.TestProcessCurrentActivityNULL());
            }
        }
    }

}

