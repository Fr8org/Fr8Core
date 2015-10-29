using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Hub.Managers.APIManagers.Packagers.Json
{
    class JsonContent : StringContent
    {
        public JsonContent(object content)
            : base(JsonConvert.SerializeObject(content))
        {
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        public static JsonContent FromObject(object content)
        {
            if (content == null)
                return null;
            return new JsonContent(content);
        }
    }
}