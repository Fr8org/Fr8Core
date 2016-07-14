using Newtonsoft.Json;

namespace PlanDirectory.Infrastructure
{
    public abstract class TemplateTag
    {
        [JsonIgnore]
        public abstract string Title { get; }
    }
}