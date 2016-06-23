using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace PlanDirectory.Infrastructure
{
    public class ActivityTemplateTag : TemplateTag
    {
        //id, WebServiceId, name
        private List<Tuple<Guid, int, string>> _values = new List<Tuple<Guid, int, string>>();

        [JsonIgnore]
        public override string Title { get { return string.Join(", ", _values.Select(a => a.Item3).ToArray()); } }

        public ActivityTemplateTag(List<ActivityTemplateDTO> values)
        {
            values.ForEach(a =>
            { _values.Add(new Tuple<Guid, int, string>(a.Id, a.WebService.Id, a.Name)); });
        }
    }
}