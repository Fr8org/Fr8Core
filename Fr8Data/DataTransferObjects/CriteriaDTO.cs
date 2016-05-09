using Newtonsoft.Json.Linq;

namespace Fr8Data.DataTransferObjects
{
    /// <summary>Data transfer object for Criteria entity.</summary>
    public class CriteriaDTO
    {
        /// <summary>Criteria identity.</summary>
        public int Id { get; set; }
                
        /// <summary>Whether to execute action when conditions are satisfied or execute without using conditions.</summary>
        public int ExecutionType { get; set; }

        /// <summary>JSON array with condition tuples.</summary>
        public JToken Conditions { get; set; }
    }
}