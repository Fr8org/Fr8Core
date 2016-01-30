using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalDropboxTests.Integration
{
        [Explicit]
        public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
            private const int ActionCount = 1;
            private const string Get_File_List_Action_Name = "Get_File_List";


            public override string TerminalName
            {
                get { return "terminalDropbox"; }
            }

            [Test, CategoryAttribute("Integration.terminalDropbox")]
            public async void Discover_Check_Returned_Actions()
            {
                //Arrange
                var discoverUrl = GetTerminalDiscoverUrl();

                //Act
                var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

                //Assert
                Assert.IsNotNull(terminalDiscoverResponse, "Terminal Dropbox discovery did not happen.");
                Assert.IsNotNull(terminalDiscoverResponse.Actions, "Dropbox terminal actions were not loaded");
                Assert.AreEqual(ActionCount, terminalDiscoverResponse.Actions.Count,
                "Not all terminal Dropbox actions were loaded");
                Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Get_File_List_Action_Name), true,"Action " + Get_File_List_Action_Name + " was not loaded");

            }
        }
}
