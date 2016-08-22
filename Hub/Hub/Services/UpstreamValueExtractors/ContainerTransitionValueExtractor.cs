using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Services.UpstreamValueExtractors
{
    public class ContainerTransitionValueExtractor : UpstreamValueExtractorBase<ContainerTransition>
    {
        protected override void ExtractUpstreamValue(ContainerTransition containerTransition, ICrateStorage crateStorage)
        {
            foreach (var transition in containerTransition.Transitions)
            {
                foreach (var condition in transition.Conditions)
                {
                    var fieldValue = GetValue(crateStorage, new FieldDTO(condition.Field));

                    if (fieldValue != null)
                    {
                        containerTransition.ResolvedUpstreamFields.Add(new KeyValueDTO(condition.Field, fieldValue.ToString()));
                    }
                }
            }
        }
    }
}
