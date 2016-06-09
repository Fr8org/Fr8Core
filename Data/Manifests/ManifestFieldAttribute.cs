using System;

namespace Fr8Data.Manifests
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ManifestFieldAttribute : Attribute
    {
        // User-friendly name
        public string Label { get; set; }
        // Flag indicating wether this field should be automatically published in available fields when corresponding manifest is published as available
        public bool IsHidden { get; set; }

        public ManifestFieldAttribute()
        {
        }

        public ManifestFieldAttribute(string label, bool isHidden = false)
        {
            Label = label;
            IsHidden = isHidden;
        }
    }
}
