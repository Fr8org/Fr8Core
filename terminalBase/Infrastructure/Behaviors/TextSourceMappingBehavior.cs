using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;

namespace TerminalBase.Infrastructure.Behaviors
{
    public class TextSourceMappingBehavior
    {
        public const string ConfigurationControlsLabel = "Configuration_Controls";
        public const string BehaviorPrefix = "TextSourceMappingBehavior-";


        private ICrateManager _crateManager;
        private CrateStorage _crateStorage;
        private string _behaviorName;

        public TextSourceMappingBehavior(
            CrateStorage crateStorage,
            string behaviorName)
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _crateStorage = crateStorage;
            _behaviorName = behaviorName;
        }

        public CrateStorage CrateStorage
        {
            get { return _crateStorage; }
        }

        private StandardConfigurationControlsCM GetOrCreateStandardConfigurationControlsCM()
        {
            var controlsCM = _crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .FirstOrDefault();

            if (controlsCM == null)
            {
                var crate = _crateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
                _crateStorage.Add(crate);

                controlsCM = crate.Content;
            }

            return controlsCM;
        }

        private bool IsBehaviorControl(ControlDefinitionDTO control)
        {
            return control.Name != null && control.Name.StartsWith(BehaviorPrefix);
        }

        private string GetFieldId(ControlDefinitionDTO control)
        {
            return control.Name.Substring(BehaviorPrefix.Length);
        }

        public void Clear()
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var textSources = controlsCM
                .Controls
                .Where(IsBehaviorControl)
                .OfType<TextSource>()
                .ToList();

            foreach (var textSource in textSources)
            {
                controlsCM.Controls.Remove(textSource);
            }
        }

        public void Append(IEnumerable<string> fieldIds, string upstreamSourceLabel)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            foreach (var fieldId in fieldIds)
            {
                var name = string.Concat(BehaviorPrefix, fieldId);

                var textSource = new TextSource(fieldId, upstreamSourceLabel, name);
                controlsCM.Controls.Add(textSource);
            }
        }

        public IDictionary<string, string> GetValues(CrateStorage payload = null)
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
                    value = textSource.GetValue(payload ?? _crateStorage);
                }
                catch { }
                result.Add(fieldId, value);
            }

            return result;
        }

        
    }
}
