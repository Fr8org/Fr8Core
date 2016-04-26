using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class HistoryItemDTO
    {
        public string Activity { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public string Fr8UserId { get; set; }
        public string Data { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public string ObjectId { get; set; }
        public string Component { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Status { get; set; }
    }

    public class HistoryQueryDTO
    {
        [JsonProperty("page")]
        public int? Page { get; set; }

        [JsonProperty("isDescending")]
        public bool? IsDescending { get; set; }

        [JsonProperty("isCurrentUser")]
        public bool IsCurrentUser { get; set; }

        [JsonProperty("itemPerPage")]
        public int? ItemPerPage { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }
    }

    public class HistoryResultDTO
    {
        [JsonProperty("items")]
        public IList<HistoryItemDTO> Items { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalItemCount")]
        public int TotalItemCount { get; set; }
    }
}
