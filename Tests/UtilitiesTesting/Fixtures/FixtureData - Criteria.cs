using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                LastUpdated = DateTime.Now,
                ProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1()
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
                LastUpdated = DateTime.Now,
                
            };
            return criteriaDO;
        }
    }
}
