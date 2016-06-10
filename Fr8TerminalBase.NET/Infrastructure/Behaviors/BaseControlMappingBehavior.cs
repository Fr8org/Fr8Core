using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using StructureMap;

namespace Fr8.TerminalBase.Infrastructure.Behaviors
{
    public abstract class BaseControlMappingBehavior<T> where T : ControlDefinitionDTO
    {
        public const string ConfigurationControlsLabel = "Configuration_Controls";
        public static string BehaviorPrefix = "";

        protected ICrateStorage _crateStorage;
        protected string _behaviorName;
        
        protected BaseControlMappingBehavior(ICrateStorage crateStorage,string behaviorName)
        {
            _crateStorage = crateStorage;
            BehaviorPrefix = behaviorName;
        }

        public ICrateStorage CrateStorage
        {
            get { return _crateStorage; }
        }
        protected StandardConfigurationControlsCM GetOrCreateStandardConfigurationControlsCM()
        {
            var controlsCM = _crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .FirstOrDefault();

            if (controlsCM == null)
            {
                controlsCM = new StandardConfigurationControlsCM();
                _crateStorage.Add(Crate.FromContent(ConfigurationControlsLabel, controlsCM));
            }

            return controlsCM;
        }

        protected bool IsBehaviorControl(ControlDefinitionDTO control)
        {
            return control.Name != null && control.Name.StartsWith(BehaviorPrefix);
        }

        protected string GetFieldId(ControlDefinitionDTO control)
        {
            return control.Name.Substring(BehaviorPrefix.Length);
        }

        public void Clear()
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var controls = controlsCM.Controls.Where(IsBehaviorControl)
                .OfType<T>()
                .ToList();

            foreach (var control in controls)
            {
                controlsCM.Controls.Remove(control);
            }
        }
    }
}
