using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
    public class AvailableDataDTO
    {
        public readonly List<FieldDTO> AvailableFields = new List<FieldDTO>();
        public readonly List<CrateDescriptionDTO> AvailableCrates = new List<CrateDescriptionDTO>();
    }
}