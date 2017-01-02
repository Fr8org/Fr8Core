using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalZendesk.Models
{

    public class ZendeskUser
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("extended_role")]
        public string ExtendedRole { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
    public class TicketCreatedEventPayload
    {
        [JsonProperty("via")]
        public string Via { get; set; }

        [JsonProperty("ticket_type")]
        public string TicketType { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("assignee")]
        public ZendeskUser Assignee { get; set; }

        [JsonProperty("id")]
        public string Id { get;set;}

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("updated_at_with_timestamp")]
        public string UpdateAtWithTimestamp { get; set; }

        [JsonProperty("current_user")]
        public ZendeskUser CurrentUser { get; set; }

        [JsonProperty("organization_name")]
        public string OrganizationName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("due_date")]
        public string DueDate { get; set; }

        [JsonProperty("due_date_with_timestamp")]
        public string DueDateWithTimestamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("cc_names")]
        public string CCNames { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("requester")]
        public ZendeskUser Requester { get; set; }

        [JsonProperty("created_at_with_timestamp")]
        public string CreatedAtWithTimestamp { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }
    }
}