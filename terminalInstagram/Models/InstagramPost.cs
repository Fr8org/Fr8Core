using Newtonsoft.Json;

namespace terminalInstagram.Models
{

    public class InstagramPost
    {
        [JsonProperty("data")]
        public InstagramPostData data { get; set; }
        [JsonProperty("meta")]
        public InstagramMeta meta { get; set; }
    }

    public class InstagramMeta
    {
        [JsonProperty("code")]
        public int code { get; set; }
    }
    public class InstagramPostData
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("location")]
        public string location { get; set; }

        [JsonProperty("story")]
        public string[] tags{ get; set; }

        [JsonProperty("comments")]
        public InstagramComment comments { get; set; }

        [JsonProperty("filter")]
        public string filter { get; set; }

        [JsonProperty("created_time")]
        public string createdTime { get; set; }

        [JsonProperty("link")]
        public string link { get; set; }

        [JsonProperty("likes")]
        public InstagramLikes likes { get; set; }

        [JsonProperty("users_in_photo")]
        public InstagramUser[] usersInPhoto { get; set; }

        [JsonProperty("caption")]
        public Caption caption { get; set; }

        [JsonProperty("user_has_liked")]
        public bool userHasLiked{ get; set; }

        [JsonProperty("user")]
        public InstagramUser user { get; set; }

        [JsonProperty("images")]
        public InstagramImageList instagramImage { get; set; }

    }

    public class Caption
    {
        [JsonProperty("created_time")]
        public string createdTime { get; set; }
        [JsonProperty("text")]
        public string text { get; set; }
        [JsonProperty("from")]
        public InstagramUser from { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
    }

    public class InstagramComment
    {
        [JsonProperty("count")]
        public int count { get; set; }
    }

    public class InstagramLikes
    {
        [JsonProperty("likes")]
        public int count { get; set; }
    }

    public class InstagramImageList
    {
        [JsonProperty("low_resolution")]
        public InstagramImage lowResolution { get; set; }
        [JsonProperty("thumbnail")]
        public InstagramImage thumbnail { get; set; }
        [JsonProperty("standard_resolution")]
        public InstagramImage standardResolution { get; set; }

    }
    public class InstagramImage
    {
        [JsonProperty("url")]
        public string url { get; set; }
        [JsonProperty("width")]
        public string width { get; set; }
        [JsonProperty("height")]
        public string height { get; set; }
    }

    public class InstagramUser
    {
        [JsonProperty("username")]
        public string username { get; set; }
        [JsonProperty("profil_picture")]
        public string profilPicture { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("full_name")]
        public string fullName { get; set; }
    }
}