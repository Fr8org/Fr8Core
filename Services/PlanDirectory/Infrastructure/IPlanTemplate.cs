using Fr8Data.DataTransferObjects.PlanTemplates;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        void Publish(PlanTemplateDTO planTemplate);
        void Unpublish(PlanTemplateDTO planTemplate);
    }
}
