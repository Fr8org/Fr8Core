using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	partial class FixtureData
	{
		private  const string _xmlPayLoadLocation = "DockyardTest\\Content\\DocusignXmlPayload.xml";
        
        public static ProcessDO TestProcess1()
		{
			var process = new ProcessDO();
			process.Id = 49;
            process.ProcessTemplateId = TestProcessTemplate2().Id;
			process.ProcessState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
			return process;
		}

        public static ProcessDO TestHealthDemoProcess1()
        {
            var process = new ProcessDO();
            process.Id = 49;
            process.ProcessTemplateId = TestProcessTemplate2().Id;
            process.ProcessState = ProcessState.Executing;
            process.ProcessNodes.Add(TestProcessNode1());
            return process;
        }

        public static IList<ProcessDO> GetProcesses()
		{
			IList<ProcessDO> processList = new List<ProcessDO>();
            var processTemplateId = TestProcessTemplate2().Id;
			processList.Add(new ProcessDO()
			{
				Id = 1,
				Name = "Process 1",
                ProcessTemplateId = processTemplateId,
				DockyardAccountId = "testuser",
				ProcessState = ProcessState.Executing
			});

			processList.Add(new ProcessDO()
			{
				Id = 2,
				Name = "Process 2",
                ProcessTemplateId = processTemplateId,
				DockyardAccountId = "testuser",
				ProcessState = ProcessState.Executing
			});

			processList.Add(new ProcessDO()
			{
				Id = 3,
				Name = "Process 3",
                ProcessTemplateId = processTemplateId,
				DockyardAccountId = "testuser",
				ProcessState = ProcessState.Unstarted
			});

			processList.Add(new ProcessDO()
			{
				Id = 4,
				Name = "Process 4",
                ProcessTemplateId = processTemplateId,
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

        public static ProcessDO TestProcesswithCurrentActivityAndNextActivity()
        {
            var process = new ProcessDO();
            process.Id = 49;
				process.ProcessTemplate = TestProcessTemplate2();
            process.ProcessTemplateId = TestProcessTemplate2().Id;
            process.ProcessState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentActivity = FixtureData.TestAction7();
				process.NextActivity = FixtureData.TestAction10();
            return process;
        }

        public static ProcessDO TestProcessCurrentActivityNULL()
        {
            var process = new ProcessDO();
            process.Id = 49;
            process.ProcessTemplateId = TestProcessTemplate2().Id;
            process.ProcessState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentActivity = null;
            return process;
        }

        public static ProcessDO TestProcesswithCurrentActivityAndNextActivityTheSame()
        {
            var process = new ProcessDO();
            process.Id = 49;
            process.ProcessTemplateId = TestProcessTemplate2().Id;
            process.ProcessState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentActivity = FixtureData.TestAction7();
            process.NextActivity = FixtureData.TestAction7();
            return process;
        }

        public static ProcessDO TestProcessSetNextActivity()
        {
            var process = new ProcessDO();
            process.Id = 49;
            process.ProcessTemplateId = TestProcessTemplate2().Id;
            process.ProcessState = 1;
            process.ProcessNodes.Add(TestProcessNode1());
            process.CurrentActivity = FixtureData.TestAction7();
            process.NextActivity = null;
            return process;
        }
	}
}