using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static AspNetRolesDO TestRole()
        {
            return new AspNetRolesDO()
            {                
                Name = "Test Role 1"                
            };
        }        
    }
}

