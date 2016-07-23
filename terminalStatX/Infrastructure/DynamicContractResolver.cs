using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace terminalStatX.Infrastructure
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly string[] _propertyNamesToExclude;

        public DynamicContractResolver(string[] propertyNameToExclude)
        {
            _propertyNamesToExclude = propertyNameToExclude;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            if(_propertyNamesToExclude != null)
                properties = properties.Where(p => !_propertyNamesToExclude.ToList().Contains(p.PropertyName)).ToList();

            return properties;
        }
    }
}