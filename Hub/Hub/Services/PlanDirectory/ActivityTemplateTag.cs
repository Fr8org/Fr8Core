using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Hub.Services.PlanDirectory
{
    public class ActivityTemplateTag : TemplateTag
    {
        //id, WebServiceId, name
        private List<Tuple<Guid, Guid, string>> _values = new List<Tuple<Guid, Guid, string>>();

        [JsonIgnore]
        public override string Title { get { return string.Join(", ", _values.Select(a => a.Item3).ToArray()); } }

        public ActivityTemplateTag(List<ActivityTemplateDTO> values)
        {
            values.ForEach(a =>
            {
                a.Categories.ToList().ForEach(c =>
                {
                    _values.Add(new Tuple<Guid, Guid, string>(a.Id, c.Id, a.Name));
                });
            });
        }
    }
}