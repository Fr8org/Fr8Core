using Newtonsoft.Json;

namespace Hub.Data
{
    public abstract class TemplateTag
    {
        [JsonIgnore]
        public abstract string Title { get; }
    }
}