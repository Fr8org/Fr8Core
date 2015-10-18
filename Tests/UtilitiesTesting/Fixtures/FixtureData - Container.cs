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
            var containerDO = new ContainerDO();
            containerDO.Id = 49;
            containerDO.RouteId = TestRoute2().Id;
            containerDO.ContainerState = 1;
            containerDO.ProcessNodes.Add(TestProcessNode1());
            return containerDO;
		}

        public static ContainerDO TestHealthDemoContainer1()
        {
            var containerDO = new ContainerDO();
            containerDO.Id = 49;
            containerDO.RouteId = TestRoute2().Id;
            containerDO.ContainerState = ContainerState.Executing;
            containerDO.ProcessNodes.Add(TestProcessNode1());
            return containerDO;
        }

        public static IList<ContainerDO> GetContainers()
		{
            IList<ContainerDO> containeList = new List<ContainerDO>();
            var routeId = TestRoute2().Id;
            containeList.Add(new ContainerDO()
			{
				Id = 1,
				Name = "Process 1",
                RouteId = routeId,
				Fr8AccountId = "testuser",
				ContainerState = ContainerState.Executing
			});

            containeList.Add(new ContainerDO()
			{
				Id = 2,
				Name = "Process 2",
                RouteId = routeId,
				Fr8AccountId = "testuser",
				ContainerState = ContainerState.Executing
			});

            containeList.Add(new ContainerDO()
			{
				Id = 3,
				Name = "Process 3",
                RouteId = routeId,
				Fr8AccountId = "testuser",
				ContainerState = ContainerState.Unstarted
			});

            containeList.Add(new ContainerDO()
			{
				Id = 4,
				Name = "Process 4",
                RouteId = routeId,
				Fr8AccountId = "anotheruser",
				ContainerState = ContainerState.Unstarted
			});

            return containeList;
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
            var container = new ContainerDO();
            container.Id = 49;
				container.Route = TestRoute2();
            container.RouteId = TestRoute2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentRouteNode = FixtureData.TestAction7();
				container.NextRouteNode = FixtureData.TestAction10();
            return container;
        }

        public static ContainerDO TestContainerCurrentActivityNULL()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.RouteId = TestRoute2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentRouteNode = null;
            return container;
        }

        public static ContainerDO TestContainerWithCurrentActivityAndNextActivityTheSame()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.RouteId = TestRoute2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentRouteNode = FixtureData.TestAction7();
            container.NextRouteNode = FixtureData.TestAction7();
            return container;
        }

        public static ContainerDO TestContainerSetNextActivity()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.RouteId = TestRoute2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentRouteNode = FixtureData.TestAction7();
            container.NextRouteNode = null;
            return container;
        }

        public static ContainerDO TestContainerUpdateNextActivity()
        {
            var container = new ContainerDO();
            container.Id = 49;
            container.RouteId = TestRoute2().Id;
            container.ContainerState = 1;
            container.ProcessNodes.Add(TestProcessNode1());
            container.CurrentRouteNode = FixtureData.TestAction8(null);
            container.NextRouteNode = null;
            return container;
        }

        public static ContainerDO TestContainerExecute()
        {
            var containerDO = new ContainerDO();
            containerDO.Id = 49;
            containerDO.Route = FixtureData.TestRoute2();
            containerDO.RouteId = containerDO.Route.Id;
            containerDO.ContainerState = 1;
            containerDO.ProcessNodes.Add(FixtureData.TestProcessNode1());
            return containerDO;
        }
	}
}