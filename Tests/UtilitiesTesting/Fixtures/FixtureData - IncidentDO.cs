using Data.Entities;
using Data.States;

using System;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static IncidentDO TestIncidentDO()
        {
            var curIncidentDO = new IncidentDO()
            {
                ObjectId = "Terminal Incident",
                Fr8UserId = "not_applicable",
                Data = "service_start_up",
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup",
                CreateDate = DateTimeOffset.UtcNow.AddDays(-1),
                Priority = 1
            };
           
            return curIncidentDO;
        }
    
    }
}