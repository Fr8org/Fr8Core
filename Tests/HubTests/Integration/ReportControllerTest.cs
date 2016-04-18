using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Managers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using HealthMonitor.Utility;
using Hub.Services;
using System.Data.Entity.Infrastructure;

namespace HubTests.Integration
{
    public class ReportControllerTest : BaseTerminalIntegrationTest
    {

        public override string TerminalName
        {
            get { return "Hub"; }
        }

        // Note: Before starting up the test, comment out user identity-related code
        // in Report.cs. Otherwise, an exception will be thrown. 
        // A better option would be to add the code which would create and assign 
        // User identity to the thread.  

        [Test, Ignore]
        public async Task HistoryStressTest()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => GetRecords()));
            }
            await Task.WhenAll(tasks);
        }

        private void GetRecords()
        {
            var report = new Report();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ((IObjectContextAdapter)uow.Db).ObjectContext.CommandTimeout = 6000; //100 minutes
                report.GetAllFacts(uow);
            }
        }
    }
}
