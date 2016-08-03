using System;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class UserDTO
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("emailAddressId")]
        public int EmailAddressID { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("organizationId")]
        public int? organizationId { get; set; }

        [JsonProperty("profileId")]
        public Guid ProfileId { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }
    }
}
