using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        private const string _xmlPayLoadLocation = "DockyardTest\\Content\\DocusignXmlPayload.xml";

        public static ContainerDO TestProcess1()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            return container;
        }

        public static ContainerDO TestHealthDemoProcess1()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = ContainerState.Executing;
            container.ProcessNodes.Add(TestProcessNode1());
            return container;
        }

        public static IList<ContainerDO> GetProcesses()
        {
            IList<ContainerDO> containerList = new List<ContainerDO>();
            var processTemplateId = TestProcessTemplate2().Id;
            containerList.Add(new ContainerDO()
            {
                Id = 1,
                Name = "Process 1",
                ProcessTemplateId = processTemplateId,
                DockyardAccountId = "testuser",
                ContainerState = ContainerState.Executing
            });

            containerList.Add(new ContainerDO()
            {
                Id = 2,
                Name = "Process 2",
                ProcessTemplateId = processTemplateId,
                DockyardAccountId = "testuser",
                ContainerState = ContainerState.Executing
            });

            containerList.Add(new ContainerDO()
            {
                Id = 3,
                Name = "Process 3",
                ProcessTemplateId = processTemplateId,
                DockyardAccountId = "testuser",
                ContainerState = ContainerState.Unstarted
            });

            containerList.Add(new ContainerDO()
            {
                Id = 4,
                Name = "Process 4",
                ProcessTemplateId = processTemplateId,
                DockyardAccountId = "anotheruser",
                ContainerState = ContainerState.Unstarted
            });

            return containerList;
        }

        /// <summary>
        /// Determines physical location of XML file with test data contents 
        /// </summary>
        /// <param name="physLocation"></param>
        /// <returns></returns>
        public static string FindXmlPayloadFullPath(string physLocation, string filepath = "DockyardTest\\Content\\DocusignXmlPayload.xml")
        {
            if (string.IsNullOrEmpty(physLocation))
                return string.Empty;

            string path = Path.Combine(physLocation, filepath);
            if (!File.Exists(path))
                path = FindXmlPayloadFullPath(UpNLevels(physLocation, 1), filepath);
            return path;
        }

        /// <summary>
        /// Given a directory path, returns an upper level path by the specified number of levels up.
        /// </summary>
        private static string UpNLevels(string path, int levels)
        {
            int index = path.LastIndexOf('\\', path.Length - 1, path.Length);
            if (index <= 3)
                return string.Empty;
            string result = path.Substring(0, index);
            if (levels > 1)
            {
                result = UpNLevels(result, levels - 1);
            }
            return result;
        }

        public static ContainerDO TestProcesswithCurrentActivityAndNextActivity()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplate = TestProcessTemplate2();
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentActivity = FixtureData.TestAction7();
            container.NextActivity = FixtureData.TestAction10();
            return container;
        }

        public static ContainerDO TestProcessCurrentActivityNULL()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentActivity = null;
            return container;
        }

        public static ContainerDO TestProcesswithCurrentActivityAndNextActivityTheSame()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentActivity = FixtureData.TestAction7();
            container.NextActivity = FixtureData.TestAction7();
            return container;
        }

        public static ContainerDO TestProcessSetNextActivity()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentActivity = FixtureData.TestAction7();
            container.NextActivity = null;
            return container;
        }

        public static ContainerDO TestProcessUpdateNextActivity()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.ProcessTemplateId = TestProcessTemplate2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentActivity = FixtureData.TestAction8(null);
            container.NextActivity = null;
            return container;
        }

        public static ContainerDO TestProcessExecute()
        {
            var containerDO = new ContainerDO();
            containerDO.Id = 49;
            containerDO.ProcessTemplate = FixtureData.TestProcessTemplate2();
            containerDO.ProcessTemplateId = containerDO.ProcessTemplate.Id;
            containerDO.ContainerState = 1;
            containerDO.ProcessNodes.Add(FixtureData.TestProcessNode1());
            return containerDO;
        }
    }
}