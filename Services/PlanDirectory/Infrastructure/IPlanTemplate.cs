using Data.Interfaces.Manifests;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        void Publish(PlanTemplateCM planTemplate);
        void Unpublish(PlanTemplateCM planTemplate);
    }
}
