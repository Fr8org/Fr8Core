using System.Collections.Generic;

namespace PlanDirectory.Infrastructure
{
    public class TemplateTagStorage
    {
        public TemplateTagStorage()
        {
            WebServiceTemplateTags = new List<WebServiceTemplateTag>();
            ActivityTemplateTags = new List<ActivityTemplateTag>();
        }
        public ICollection<WebServiceTemplateTag> WebServiceTemplateTags { get; set; }

        public ICollection<ActivityTemplateTag> ActivityTemplateTags { get; set; }
    }
}