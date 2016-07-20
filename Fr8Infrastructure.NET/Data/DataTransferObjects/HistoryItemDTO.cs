using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class HistoryItemDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("activity")]
        public string Activity { get; set; }
        [JsonProperty("createDate")]
        public DateTimeOffset CreateDate { get; set; }
        [JsonProperty("fr8UserId")]
        public string Fr8UserId { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("objectId")]
        public string ObjectId { get; set; }
        [JsonProperty("component")]
        public string Component { get; set; }
        [JsonProperty("primaryCategory")]
        public string PrimaryCategory { get; set; }
        [JsonProperty("secondaryCategory")]
        public string SecondaryCategory { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class IncidentDTO : HistoryItemDTO
    {
        [JsonProperty("priority")]
        public int Priority { get; set; }
        [JsonProperty("isHighPriority")]
        public bool isHighPriority { get; set; }
        
    }

    public class FactDTO : HistoryItemDTO
    {
        [JsonProperty("createdByID")]
        public string CreatedByID { get; set; }
    }

    public class HistoryQueryDTO
    {
        /// <summary>
        /// Ordinal number of subset of log records to retrieve
        /// </summary>
        [JsonProperty("page")]
        public int? Page { get; set; }
        /// <summary>
        /// Whether to perform sort of results in descending order
        /// </summary>
        [JsonProperty("isDescending")]
        public bool? IsDescending { get; set; }
        /// <summary>
        /// Whether to show log records of current user only
        /// </summary>
        [JsonProperty("isCurrentUser")]
        public bool IsCurrentUser { get; set; }
        /// <summary>
        /// Max number of log records to retrieve in single response
        /// </summary>
        [JsonProperty("itemPerPage")]
        public int? ItemPerPage { get; set; }
        /// <summary>
        /// Part of textual field of log record to filter by
        /// </summary>
        [JsonProperty("filter")]
        public string Filter { get; set; }
    }

    public class HistoryResultDTO<T> where T : HistoryItemDTO
    {
        [JsonProperty("items")]
        public IList<T> Items { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalItemCount")]
        public int TotalItemCount { get; set; }
    }
}
