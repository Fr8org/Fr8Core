using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace Fr8.TerminalBase.Infrastructure.Behaviors
{
    public class TextSourceMappingBehavior : BaseControlMappingBehavior<TextSource>
    {
        private readonly bool _reqeustUpstream;

        public TextSourceMappingBehavior(ICrateStorage crateStorage, string behaviorName, bool requestUpstream)
            : base(crateStorage, behaviorName)
        {
            _reqeustUpstream = requestUpstream;

            //BehaviorPrefix = "TextSourceMappingBehavior-";
        }

        public void Append(IEnumerable<string> fieldIds, string upstreamSourceLabel, AvailabilityType availability = AvailabilityType.NotSet)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            foreach (var fieldId in fieldIds)
            {
                var name = string.Concat(BehaviorPrefix, fieldId);

                var textSource = new TextSource(fieldId, upstreamSourceLabel, name);
                if (_reqeustUpstream)
                {
                    textSource.Source = new FieldSourceDTO()
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true,
                        AvailabilityType = availability
                    };
                }

                controlsCM.Controls.Add(textSource);
            }
        }

        public IDictionary<string, string> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();
            var result = new Dictionary<string, string>();

            var textSources = controlsCM
                .Controls
                .Where(IsBehaviorControl)
                .OfType<TextSource>();

            foreach (var textSource in textSources)
            {
                var fieldId = GetFieldId(textSource);
                string value = null;
                try
                {
                    value = textSource.TextValue;
                }
                catch { }
                result.Add(fieldId, value);
            }

            return result;
        }
    }
}
