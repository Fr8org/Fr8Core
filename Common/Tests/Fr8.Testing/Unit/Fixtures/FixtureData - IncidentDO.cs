using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static IList<IncidentDO> TestIncidentsForReportControllerTest()
        {
            IList<IncidentDO> incidents = new List<IncidentDO>();
            for (int i = 0; i < 10; i++)
            {
                incidents.Add(new IncidentDO()
                {
                    Id = 1,
                    PrimaryCategory = "Incident "+i,
                    SecondaryCategory = "Test",
                    Data = i.ToString(),
                    CreateDate = DateTime.UtcNow.AddDays(i)
                });
            }
            return incidents;
        }
    }
}
