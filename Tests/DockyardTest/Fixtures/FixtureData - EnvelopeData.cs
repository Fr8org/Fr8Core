using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace DockyardTest.Fixtures
{
    partial class FixtureData
    {
        public static List<EnvelopeDataDO> CreateEnvelopeDataList()
        {
            return new List<EnvelopeDataDO>()
            {
                new EnvelopeDataDO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1" },
                new EnvelopeDataDO() { Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2" },
                new EnvelopeDataDO() { Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3" },
            };
        }
    }
}
