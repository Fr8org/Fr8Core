using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityCategoryDTO
    {
        public ActivityCategoryDTO() { }

        public ActivityCategoryDTO(string name, string iconPath)
        {
            Name = name;
            IconPath = iconPath;
        }

        public ActivityCategoryDTO(Guid id, string name, string iconPath)
        {
            Id = id;
            Name = name;
            IconPath = iconPath;
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        private sealed class NameEqualityComparer : IEqualityComparer<ActivityCategoryDTO>
        {
            public bool Equals(ActivityCategoryDTO x, ActivityCategoryDTO y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Name, y.Name);
            }
        
            public int GetHashCode(ActivityCategoryDTO obj)
            {
                return (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }
        
        private static readonly IEqualityComparer<ActivityCategoryDTO> NameComparerInstance = new NameEqualityComparer();
        
        public static IEqualityComparer<ActivityCategoryDTO> NameComparer => NameComparerInstance;
    }
}
