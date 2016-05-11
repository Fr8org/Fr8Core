using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        void Publish(PublishPlanTemplateDTO planTemplate);
        void Unpublish(PublishPlanTemplateDTO planTemplate);
    }
}
