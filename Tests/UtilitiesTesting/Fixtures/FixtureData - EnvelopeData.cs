using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static List<EnvelopeDataDTO> TestEnvelopeDataList1()
        {
            return new List<EnvelopeDataDTO>()
		           {
		               new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1"},
		               new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2"},
		               new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3"},
		           };
        }
    }
}
