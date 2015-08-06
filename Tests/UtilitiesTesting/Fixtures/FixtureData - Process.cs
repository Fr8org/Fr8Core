using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	partial class FixtureData
	{
		private static string _xmlPayLoadLocation = "DockyardTest\\Content\\DocusignXmlPayload.xml";

		public static ProcessDO TestProcess1()
		{
			var process = new ProcessDO();
			process.Id = 49;
			process.CurrentProcessNodeId = 50;
			process.ProcessState = 1;
			process.ProcessNode = TestProcessNode1();
			return process;
		}

		public static IList<ProcessDO> GetProcesses()
		{
			IList<ProcessDO> processList = new List<ProcessDO>();
			processList.Add(new ProcessDO()
			{
				Id = 1,
				Name = "Process 1",
				DockyardAccountId = "testuser",
				ProcessState = ProcessState.Executing
			});

			processList.Add(new ProcessDO()
			{
				Id = 2,
				Name = "Process 2",
				DockyardAccountId = "testuser",
				ProcessState = ProcessState.Executing
			});

			processList.Add(new ProcessDO()
			{
				Id = 3,
				Name = "Process 3",
				DockyardAccountId = "testuser",
				ProcessState = ProcessState.Unstarted
			});

			processList.Add(new ProcessDO()
			{
				Id = 4,
				Name = "Process 4",
				DockyardAccountId = "anotheruser",
				ProcessState = ProcessState.Unstarted
			});

			return processList;
		}

		/// <summary>
		/// Determines physical location of XML file with test data contents 
		/// </summary>
		/// <param name="physLocation"></param>
		/// <returns></returns>
		public static string FindXmlPayloadFullPath(string physLocation)
		{
			if (string.IsNullOrEmpty(physLocation))
				return string.Empty;

			string path = Path.Combine(physLocation, _xmlPayLoadLocation);
			if (!File.Exists(path))
				path = FindXmlPayloadFullPath(UpNLevels(physLocation, 1));
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

	}
}