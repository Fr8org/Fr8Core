using Data.Entities;

namespace DockyardTest.Fixtures
{
    partial class FixtureData
    {
        public AspNetRolesDO TestRole()
        {
            return new AspNetRolesDO()
            {                
                Name = "Test Role 1"                
            };
        }        
    }
}

