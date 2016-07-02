using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Fr8.Infrastructure.Utilities.Serializers.Json
{
    /// <summary>
    /// Default JSON serializer
    /// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
    /// </summary>
    public class JsonSerializer
    {
        public JsonSerializerSettings Settings;

        /// <summary>
        /// Default serializer
        /// </summary>
        public JsonSerializer()
        {
            Settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CustomPropertyNamesContractResolver()
            };
        }

        /// <summary>
        /// Serialize the object as JSON
        /// </summary>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        /// <summary>
        /// Deserializes JSON response to object
        /// </summary>
        public T Deserialize<T>(string responseJson)
        {
            if (responseJson == null) return default(T);
            if (responseJson.StartsWith("{\"messages\":{")) return default(T);


            return responseJson == "[]"
                ? default(T)
                : JsonConvert.DeserializeObject<T>(responseJson, Settings);
        }

        /// <summary>
        /// Deserializes JSON response to List of object
        /// </summary>
        public IList<T> DeserializeList<T>(string responseJson)
        {
            if (responseJson == null) return default(IList<T>);
            if (responseJson.StartsWith("{\"messages\":{")) return default(IList<T>);
            if (responseJson == "[]") return default(IList<T>);

            var result = JsonConvert.DeserializeObject<object>(responseJson, Settings);
            IList<T> listT = new List<T>();
            foreach (var item in ((Newtonsoft.Json.Linq.JObject)(result)))
            {
                listT.Add(Deserialize<T>(item.Value.ToString()));
            }
            return listT;
        }


        //=================================
        // The following code converts our Pascal Case property names to the kind that mandrill needs
        // based loosely on http://stackoverflow.com/a/18828097/1915866
        public enum IdentifierCase
        {
            Camel,
            Pascal,
        }
        public class CustomPropertyNamesContractResolver : DefaultContractResolver
        {
            public CustomPropertyNamesContractResolver(bool shareCache = false)
                : base(shareCache)
            {
                Case = IdentifierCase.Pascal;
                PreserveUnderscores = true;
            }

            public IdentifierCase Case { get; set; }
            public bool PreserveUnderscores { get; set; }

            protected override string ResolvePropertyName(string propertyName)
            {
                return ChangeCase(propertyName);
            }

            private string ChangeCase(string s)
            {
                var sb = new StringBuilder();

               //NOT USED RIGHT NOW bool addNoUnderscore = true;//applies for the first char, and for chars that follow a capital char (i.e. we don't want "CC" to become "C_C")
                var lastCharUpper = true;
                foreach (var c in s)
                {

                    if (char.IsLower(c))
                    {
                        sb.Append(c);
                        lastCharUpper = false;
                    }
                    else
                    {
                        if (lastCharUpper)
                        {
                            sb.Append(char.ToLower(c));
                        }
                        else
                        {
                            sb.Append("_");
                            sb.Append(char.ToLower(c));
                            lastCharUpper = true;
                        }
                    }

                }
                return sb.ToString();
            }
        }
    }
}
