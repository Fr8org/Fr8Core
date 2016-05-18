using Data.Constants;

namespace Data.Interfaces.Manifests
{
    // TODO: this will be probably merged with full implementation from FR-3146.
    public class PlanTemplateCM : Manifest
    {
        public PlanTemplateCM() : base(MT.PlanTemplate)
        {
        }

        [MtPrimaryKey]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Fr8DirectoryVisible { get; set; }
    }
}
