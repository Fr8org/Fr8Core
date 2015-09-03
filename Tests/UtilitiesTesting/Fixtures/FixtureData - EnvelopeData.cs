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

        public static string TestTeamplateId = "58521204-58AF-4E65-8A77-4F4B51FEF626";

        public static List<EnvelopeDataDTO> TestEnvelopeDataList2(string envelopeId)
        {
            return new List<EnvelopeDataDTO>()
            {
                new EnvelopeDataDTO()
                {
                     DocumentId = 1,
                     EnvelopeId = envelopeId,
                     Name = "Doctor",
                     RecipientId = "1",
                     TabId = "1",
                     Value = "Johnson"
                },
                new EnvelopeDataDTO()
                {
                     DocumentId = 1,
                     EnvelopeId = envelopeId,
                     Name = "Condition",
                     RecipientId = "1",
                     TabId = "1",
                     Value = "Marthambles"
                }
            };
        }

        public static string FieldMappings
        {
            get
            {
                return "{\"fields\": [ { \"name\": \"Doctor\", \"value\": \"[Customer].physician_string\" }, { \"name\": \"Condition\", \"value\": \"[Customer].medical_condition\"} ] }";
            }
        }
        public static string FieldMappings2
        {
            get
            {
                return "{\"fields\": [ { \"name\": \"Physicial\", \"value\": \"[Customer].physician_string\" }, { \"name\": \"Condition\", \"value\": \"[Customer].medical_condition\"} ] }";
            }
        }
    }
}
