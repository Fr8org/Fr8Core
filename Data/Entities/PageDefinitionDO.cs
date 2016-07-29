using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Data.Entities
{
    public class PageDefinitionDO : BaseObject
    {
        public PageDefinitionDO()
        {
            Tags = new string[0];
            PlanTemplatesIds = new List<string>(0);
        }
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
            get { return JsonConvert.SerializeObject(Tags.Distinct()); }
            set { Tags = JsonConvert.DeserializeObject<IEnumerable<string>>(value); }
        }

        [NotMapped]
        public Uri Url { get; set; }

        [Column("Url")]
        public string UrlString
        {
            get { return Url?.ToString(); }
            set { Url = value == null ? null : new Uri(value); }
        }
        [Required]
        public string Type { get; set; }

        public string Description { get; set; }
        public string PageName { get; set; }

        [NotMapped]
        public List<string> PlanTemplatesIds { get; set; }

        [Column("PlanTemplatesIds")]
        public string PlanTemplatesString
        {
            get { return JsonConvert.SerializeObject(PlanTemplatesIds.Distinct()); }
            set { PlanTemplatesIds = JsonConvert.DeserializeObject<List<string>>(value); }
        }
    }
}