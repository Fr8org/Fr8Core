using Data.Entities;

namespace Fr8.Testing.Unit.Fixtures
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

