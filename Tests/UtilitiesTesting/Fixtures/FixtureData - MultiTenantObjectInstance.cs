using Data.Interfaces.MultiTenantObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    public static class FixtureData___MultiTenantObjectSubClass
    {
        public static DocuSignRecipientStatusReportMTO TestData1()
        {
            return new DocuSignRecipientStatusReportMTO()
            {
                CompletedDate = DateTime.Now.ToShortDateString(),
                DeliveredDate = DateTime.Now.AddDays(1).ToShortDateString(),
                Email = "some@email.mine",
                fr8AccountId = 0,
                Name = "SomeName",
                Status = "SomeStatus"
            };
        }


    }
}
