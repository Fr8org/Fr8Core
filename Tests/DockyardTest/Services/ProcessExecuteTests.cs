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
        [Test]

        [ExpectedException(typeof(ArgumentNullException))]

        public void ExecuteFailsIfNullProcessDo()
        {
            Process processService = ObjectFactory.GetInstance<Process>();

            ProcessDO processDo = null;

            processService.Execute(processDo);

        }
        
        [Test]

        [ExpectedException(typeof(ArgumentNullException))]

        public void ExecuteFailsIfNullProcessDoCurrentActivity()
        {
            //All static parcial class for already prepared data
            //The CurrentActivity value is already set to null
            ProcessDO processDo = FixtureData.TestProcessCurrentActivityNULL();

            Process processService = ObjectFactory.GetInstance<Process>();

            processService.Execute(processDo);

        }
    }

}

