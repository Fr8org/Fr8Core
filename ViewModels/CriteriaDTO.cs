using Newtonsoft.Json.Linq;

namespace Web.ViewModels
{
    public class CriteriaDTO
    {
        public int Id { get; set; }
        public int ProcessTemplateId { get; set; }
        public string Name { get; set; }
        public int ExecutionMode { get; set; }
        public JObject Conditions { get; set; }
    }
}