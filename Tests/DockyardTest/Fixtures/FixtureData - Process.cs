using System.Collections.Generic;
using Data.Entities;
using Data.States;

namespace DockyardTest.Fixtures
{
    partial class FixtureData
    {
        public static IList<ProcessDO> GetProcesses()
        {
            IList<ProcessDO> processList = new List<ProcessDO>();
            processList.Add(new ProcessDO()
            {
                Id = 1,
                Name = "Process 1",
                UserId = "testuser",
                Description = "Process 1 Description",
                ProcessState = ProcessState.Unstarted
            });

            processList.Add(new ProcessDO()
            {
                Id = 2,
                Name = "Process 2",
                UserId = "testuser",
                Description = "Process 2 Description",
                ProcessState = ProcessState.Processing
            });

            processList.Add(new ProcessDO()
            {
                Id = 3,
                Name = "Process 3",
                UserId = "testuser",
                Description = "Process 3 Description",
                ProcessState = ProcessState.Unstarted
            });

            processList.Add(new ProcessDO()
            {
                Id = 4,
                Name = "Process 4",
                UserId = "anotheruser",
                Description = "Process 4 Description",
                ProcessState = ProcessState.Unstarted
            });

            return processList;
        }
    }
}

