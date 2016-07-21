using System;

namespace Fr8.Infrastructure.Data.Manifests
{
    /// <summary>
    /// Allows to specify aspects of how the certain manifest field is being signaled when default setting for available crates signalling are used. 
    /// https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/ManifestFieldAttribute.md
    /// </summary>
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
