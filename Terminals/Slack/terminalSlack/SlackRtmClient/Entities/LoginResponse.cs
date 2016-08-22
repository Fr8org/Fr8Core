using System.Collections.Generic;
using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class LoginResponse
    {
        public bool Ok { get; set; }

        public Error Error { get; set; }
        public string Url { get; set; }
        public Self Self { get; set; }
        public Team Team { get; set; }

        public List<User> Users { get; set; }

        public List<Channel> Channels { get; set; }

        public List<Group> Groups { get; set; }

        public List<Im> Ims { get; set; }

        public List<Mpim> Mpims { get; set; }
    }
}
