using Newtonsoft.Json;

namespace Hub.Services.PlanDirectory
{
    public abstract class TemplateTag
    {
        [JsonIgnore]
        public abstract string Title { get; }
    }
}