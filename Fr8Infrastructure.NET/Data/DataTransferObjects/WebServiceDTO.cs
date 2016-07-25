using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
	public class WebServiceDTO
	{
	    private sealed class NameEqualityComparer : IEqualityComparer<WebServiceDTO>
	    {
	        public bool Equals(WebServiceDTO x, WebServiceDTO y)
	        {
	            if (ReferenceEquals(x, y)) return true;
	            if (ReferenceEquals(x, null)) return false;
	            if (ReferenceEquals(y, null)) return false;
	            if (x.GetType() != y.GetType()) return false;
	            return string.Equals(x.Name, y.Name);
	        }

	        public int GetHashCode(WebServiceDTO obj)
	        {
	            return (obj.Name != null ? obj.Name.GetHashCode() : 0);
	        }
	    }

	    private static readonly IEqualityComparer<WebServiceDTO> NameComparerInstance = new NameEqualityComparer();

	    public static IEqualityComparer<WebServiceDTO> NameComparer => NameComparerInstance;

	    [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }
	}
}