using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace PlanDirectory.Infrastructure
{
    public class WebServiceTemplateTag : TemplateTag
    {
        //id, iconPath, name
        private List<Tuple<int, string, string>> _values = new List<Tuple<int, string, string>>();

        public Dictionary<string, string> TagsWithIcons
        {
            get { return _values.ToDictionary(x => x.Item3, x => x.Item2); }
        }

        [JsonIgnore]
        public override string Title
        {
            get { return string.Join(", ", _values.Select(a => a.Item3).ToArray()); }
        }

        public WebServiceTemplateTag(List<WebServiceDTO> values)
        {
            values.ForEach(a =>
            { _values.Add(new Tuple<int, string, string>(a.Id, a.IconPath, a.Name)); });
        }
    }
}