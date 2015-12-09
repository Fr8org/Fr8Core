using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace terminalDocuSign.Infrastructure
{
    public class FolderItem
    {
        [JsonProperty("ownerName")]
        public string OwnerName { get; set; }
        [JsonProperty("envelopeId")]
        public string EnvelopeId { get; set; }
        [JsonProperty("envelopeUri")]
        public string EnvelopeUri { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("senderName")]
        public string SenderName { get; set; }
        [JsonProperty("senderEmail")]
        public string SenderEmail { get; set; }
        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }
        [JsonProperty("sentDateTime")]
        public DateTime SentDateTime { get; set; }
        [JsonProperty("completedDateTime")]
        public DateTime CompletedDateTime { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("shared")]
        public string Shared { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("lastModified")]
        public string LastModified { get; set; }
        [JsonProperty("pageCount")]
        public int PageCount { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("customFields")]
        public IList<CustomField> CustomFields { get; set; }
    }
}