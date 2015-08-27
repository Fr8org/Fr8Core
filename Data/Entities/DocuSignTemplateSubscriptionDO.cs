using System;

namespace Data.Entities
{
    public class DocuSignTemplateSubscriptionDO : ExternalEventSubscriptionDO,
        IEquatable<DocuSignTemplateSubscriptionDO>
    {
        public string DocuSignTemplateId { get; set; }

        public bool Equals(DocuSignTemplateSubscriptionDO other)
        {
            return ProcessTemplateId == other.ProcessTemplateId && DocuSignTemplateId == other.DocuSignTemplateId;
        }
    }
}