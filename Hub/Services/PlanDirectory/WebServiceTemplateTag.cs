using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Hub.Services.PlanDirectory
{
    public class WebServiceTemplateTag : TemplateTag
    {
        //id, iconPath, name
        private List<Tuple<Guid, string, string>> _values = new List<Tuple<Guid, string, string>>();

        public Dictionary<string, string> TagsWithIcons
        {
            get { return _values.Where(a => a.Item2 != null).ToDictionary(x => x.Item3, x => x.Item2); }
        }

        [JsonIgnore]
        public override string Title
        {
            get { return string.Join(", ", _values.Select(a => a.Item3).ToArray()); }
        }

        public WebServiceTemplateTag(List<ActivityCategoryDTO> values)
        {
            values.ForEach(a =>
            { _values.Add(new Tuple<Guid, string, string>(a.Id, a.IconPath, a.Name)); });
        }
    }
}