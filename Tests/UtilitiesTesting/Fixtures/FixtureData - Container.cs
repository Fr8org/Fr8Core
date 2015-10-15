using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	partial class FixtureData
	{
		private  const string _xmlPayLoadLocation = "DockyardTest\\Content\\DocusignXmlPayload.xml";
        
        public static ContainerDO TestContainer1()
		{
			var process = new ContainerDO();
			process.Id = 49;
            process.RouteId = TestRoute2().Id;
			process.ContainerState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
			return process;
		}

        public static ContainerDO TestHealthDemoContainer1()
        {
            var process = new ContainerDO();
            process.Id = 49;
            process.RouteId = TestRoute2().Id;
            process.ContainerState = ContainerState.Executing;
            process.ProcessNodes.Add(TestProcessNode1());
            return process;
        }

        public static IList<ContainerDO> GetContainers()
		{
			IList<ContainerDO> processList = new List<ContainerDO>();
            var processTemplateId = TestRoute2().Id;
			processList.Add(new ContainerDO()
			{
				Id = 1,
				Name = "Process 1",
                RouteId = processTemplateId,
				Fr8AccountId = "testuser",
				ContainerState = ContainerState.Executing
			});

			processList.Add(new ContainerDO()
			{
				Id = 2,
				Name = "Process 2",
                RouteId = processTemplateId,
				Fr8AccountId = "testuser",
				ContainerState = ContainerState.Executing
			});

			processList.Add(new ContainerDO()
			{
				Id = 3,
				Name = "Process 3",
                RouteId = processTemplateId,
				Fr8AccountId = "testuser",
				ContainerState = ContainerState.Unstarted
			});

			processList.Add(new ContainerDO()
			{
				Id = 4,
				Name = "Process 4",
                RouteId = processTemplateId,
				Fr8AccountId = "anotheruser",
				ContainerState = ContainerState.Unstarted
			});

			return processList;
		}

		/// <summary>
		/// Determines physical location of XML file with test data contents 
		/// </summary>
		/// <param name="physLocation"></param>
		/// <returns></returns>
		public static string FindXmlPayloadFullPath(string physLocation, string filepath="DockyardTest\\Content\\DocusignXmlPayload.xml")
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

        public static ContainerDO TestContainerWithCurrentActivityAndNextActivity()
        {
            var process = new ContainerDO();
            process.Id = 49;
				process.Route = TestRoute2();
            process.RouteId = TestRoute2().Id;
            process.ContainerState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentRouteNode = FixtureData.TestAction7();
				process.NextRouteNode = FixtureData.TestAction10();
            return process;
        }

        public static ContainerDO TestContainerCurrentActivityNULL()
        {
            var process = new ContainerDO();
            process.Id = 49;
            process.RouteId = TestRoute2().Id;
            process.ContainerState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentRouteNode = null;
            return process;
        }

        public static ContainerDO TestContainerWithCurrentActivityAndNextActivityTheSame()
        {
            var process = new ContainerDO();
            process.Id = 49;
            process.RouteId = TestRoute2().Id;
            process.ContainerState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentRouteNode = FixtureData.TestAction7();
            process.NextRouteNode = FixtureData.TestAction7();
            return process;
        }

        public static ContainerDO TestContainerSetNextActivity()
        {
            var process = new ContainerDO();
            process.Id = 49;
            process.RouteId = TestRoute2().Id;
            process.ContainerState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentRouteNode = FixtureData.TestAction7();
            process.NextRouteNode = null;
            return process;
        }

        public static ContainerDO TestContainerUpdateNextActivity()
        {
            var process = new ContainerDO();
            process.Id = 49;
            process.RouteId = TestRoute2().Id;
            process.ContainerState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentRouteNode = FixtureData.TestAction8(null);
            process.NextRouteNode = null;
            return process;
        }

        public static ContainerDO TestContainerExecute()
        {
            var processDO = new ContainerDO();
            processDO.Id = 49;
            processDO.Route = FixtureData.TestRoute2();
            processDO.RouteId = processDO.Route.Id;
            processDO.ContainerState = 1;
            processDO.ProcessNodes.Add(FixtureData.TestProcessNode1());
            return processDO;
        }
	}
}