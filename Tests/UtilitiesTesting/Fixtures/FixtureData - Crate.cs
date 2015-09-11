using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using System;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static CrateDTO CrateDTO1()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Label 1",
                Contents = "Contents 1",
                ParentCrateId = ""
            };
        }

        public static CrateDTO CrateDTO2()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Label 2",
                Contents = "Contents 2",
                ParentCrateId = ""
            };
        }
    }
}
