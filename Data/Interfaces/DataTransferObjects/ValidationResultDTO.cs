using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
    public class ValidationResultDTO
    {
        // list of controls that values are responsible for validation failure. Can be empty or null of corresponding error is activity-wide.
        public List<string> ControlNames { get; set; } = new List<string>();
        // validation error message
        public string ErrorMessage { get; set; } 
    }
}
