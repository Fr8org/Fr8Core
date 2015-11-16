using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces.DataTransferObjects
{
    public class RouteOnlyDTO
    {
        public RouteOnlyDTO()
        {
            //     SubscribedDocuSignTemplates = new List<DocuSignTemplateSubscriptionDO>();
            //     SubscribedExternalEvents = new List<ExternalEventSubscriptionDO>();
            //     DockyardAccount = new DockyardAccountDO();
            SubscribedDocuSignTemplates = new List<string>();
            SubscribedExternalEvents = new List<int?>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Tag { get; set; }

        public string Description { get; set; }
        public int RouteState { get; set; }

        public IList<string> SubscribedDocuSignTemplates { get; set; }

        public IList<int?> SubscribedExternalEvents { get; set; }
        public int StartingSubrouteId { get; set; }


        // Commented out by yakov.gnusin:
        // Do we really need to provider DockyardAccountDO inside RouteDTO?
        // We do override DockyardAccountDO in RouteController.Post action.

        // public virtual DockyardAccountDO DockyardAccount { get; set; }
        // public virtual IList<DocuSignTemplateSubscriptionDO> SubscribedDocuSignTemplates { get; set; }
        // public virtual IList<ExternalEventSubscriptionDO> SubscribedExternalEvents { get; set; }
    }
}