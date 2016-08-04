using Newtonsoft.Json;

namespace HubWeb.Infrastructure_PD.Infrastructure
{
    public abstract class TemplateTag
    {
        [JsonIgnore]
        public abstract string Title { get; }
    }
}