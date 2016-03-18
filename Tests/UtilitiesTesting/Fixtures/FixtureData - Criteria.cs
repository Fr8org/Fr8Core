using System;
using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static CriteriaDO TestCriteria1()
        {
            CriteriaDO criteriaDO = new CriteriaDO()
            {
                Id = 1,
                CriteriaExecutionType = 1,
                ConditionsJSON = @"{""criteria"":[{""field"":""Value"",""operator"":""Equals"",""value"":""test value 1""}]}",
				LastUpdated = DateTimeOffset.UtcNow,
                SubPlan = TestSubPlanDO1()
            };
            return criteriaDO;
        }

        public static CriteriaDO TestCriteriaHealthDemo()
        {
            CriteriaDO criteriaDO = new CriteriaDO()
            {
                Id = 78,
                CriteriaExecutionType = 2,
                ConditionsJSON = @"{""criteria"":[{""field"":""Value"",""operator"":""Equals"",""value"":""test value 1""}]}",
				LastUpdated = DateTimeOffset.UtcNow,
                
            };
            return criteriaDO;
        }
    }
}