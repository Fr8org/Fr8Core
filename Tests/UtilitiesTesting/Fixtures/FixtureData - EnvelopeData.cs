using System.Collections.Generic;
using Data.Entities;
using Utilities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static List<EnvelopeData> TestEnvelopeDataList1()
        {
            return new List<EnvelopeData>()
		           {
		               new EnvelopeData() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1"},
		               new EnvelopeData() {Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2"},
		               new EnvelopeData() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3"},
		           };
        }
    }
}
