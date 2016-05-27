using System;

namespace Fr8Data.Manifests
{
    public class PlanTemplateCM : Manifest
    {
        public PlanTemplateCM()
            : base(Constants.MT.PlanTemplate)
        {
        }

        [MtPrimaryKey]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// Fr8 account id.
        /// </summary>
        public string Owner { get; set; }

        public int Status { get; set; }

        public string PlanContents { get; set; }

        public Guid ParentPlanId { get; set; }
    }
}
