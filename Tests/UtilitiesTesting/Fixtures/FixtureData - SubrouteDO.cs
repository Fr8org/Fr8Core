using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static SubrouteDO TestSubrouteDO1()
        {
            var SubrouteDO = new SubrouteDO
            {
                Id = 50,
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]"
            };
            return SubrouteDO;
        }

        public static SubrouteDO TestSubrouteHealthDemo()
        {
            var SubrouteDO = new SubrouteDO
            {
                Id = 50,
                ParentRouteNodeId = 23,
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'2'}]"
            };
            return SubrouteDO;
        }

        public static SubrouteDO TestSubrouteDO2()
        {
            SubrouteDO SubrouteDO = new SubrouteDO()
            {
                Id = 50,
                Name = "TestName",
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]",
                ParentRouteNodeId = 50,
                StartingSubroute = true
            };
            return SubrouteDO;
        }

        public static SubrouteDO TestSubrouteDO3()
        {
            SubrouteDO SubrouteDO = new SubrouteDO()
            {
                Id = 50,
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return SubrouteDO;
        }

        public static SubrouteDO TestSubrouteDO4()
        {
            SubrouteDO SubrouteDO = new SubrouteDO()
            {
                Id = 1,
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return SubrouteDO;
        }
    }
}