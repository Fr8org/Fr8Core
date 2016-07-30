using System;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class PlanTemplateCM : Manifest
    {
        public PlanTemplateCM()
            : base(MT.PlanTemplate)
        {
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// Fr8 account id.
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Fr8 account name.
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Fr8Data.States.PlanTemplateStatus.
        /// </summary>
        public int Status { get; set; }

        public PlanDTO PlanContents { get; set; }

        [MtPrimaryKey]
        public string ParentPlanId { get; set; }
    }
}
