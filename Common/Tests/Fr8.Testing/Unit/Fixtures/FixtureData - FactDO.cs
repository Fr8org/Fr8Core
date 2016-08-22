using Data.Entities;
using Data.States;

using System;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static FactDO TestFactDO()
        {
            var curFactDO = new FactDO
            {
                ObjectId = "Terminal Incident",
                Fr8UserId = "not_applicable",
                Data = "service_start_up",
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup",
                CreateDate = DateTimeOffset.UtcNow.AddDays(-1) 
            };
            return curFactDO;
        }
    
    }
}