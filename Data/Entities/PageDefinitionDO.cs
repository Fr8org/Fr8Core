using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Utility;

namespace Data.Entities
{
    public class PageDefinitionDO : BaseObject
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        [NotMapped]
        public IEnumerable<string> Tags { get; set; }

        [Column("Tags")]
        [Required]
        public string TagsString
        {
            get { return Tags.Any() ? string.Join(", ", Tags) : string.Empty; }
            set
            {
                Tags = value == null
                    ? null
                    : new List<string>(value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        [NotMapped]
        public Uri Url { get; set; }

        [Column("Url")]
        public string UriString
        {
            get { return Url?.ToString(); }
            set { Url = value == null ? null : new Uri(value); }
        }
        [Required]
        public string Type { get; set; }

        public string Description { get; set; }
        public string PageName { get; set; }
    }
}