using System.Collections.Generic;

namespace PlanDirectory.Infrastructure
{
    public class TemplateTagStorage
    {
        public ICollection<WebServiceTemplateTag> WebServiceTemplateTags { get; set; }

        public ICollection<ActivityTemplateTag> ActivityTemplateTags { get; set; }
    }
}