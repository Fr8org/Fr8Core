using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    public static class FixtureData___MultiTenantObjectSubClass
    {
        public static DocuSignEnvelopeCM TestData1()
        {
            return new DocuSignEnvelopeCM()
            {
                EnvelopeId = "1",
                CompletedDate = DateTime.UtcNow.ToShortDateString(),
				DeliveredDate = DateTime.UtcNow.AddDays(1).ToShortDateString(),
                Status = "delivered"
            };
        }


    }
}
