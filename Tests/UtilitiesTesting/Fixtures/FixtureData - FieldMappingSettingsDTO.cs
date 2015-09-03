using System.Collections.Generic;
using System.Net.NetworkInformation;
using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static FieldMappingSettingsDTO TestFieldMappingSettingsDTO_Health()
        {
            return new FieldMappingSettingsDTO
            {
                Fields = new List<FieldMappingDTO>
                {
                    TestFieldMappingDTO_Doctor(),
                    TestFieldMappingDTO_Condition()
                }
            };
        }

        public static FieldMappingDTO TestFieldMappingDTO_Doctor()
        {
            return new FieldMappingDTO
            {
                Name = "Doctor",
                Value = "[Customer].Physician"
            };
        }

        public static FieldMappingDTO TestFieldMappingDTO_Condition()
        {
            return new FieldMappingDTO
            {
                Name = "Condition",
                Value = "[Customer].Condition"
            };
        }
    }
}
