using Newtonsoft.Json;

namespace terminalBasecamp2.Data
{
    public class Identity
    {
        public int Id { get; set; }
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
                {
                    return EmailAddress;
                }
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    return LastName;
                }
                if (string.IsNullOrWhiteSpace(LastName))
                {
                    return FirstName;
                }
                return $"{FirstName} {LastName}";
            }
        }
    }
}