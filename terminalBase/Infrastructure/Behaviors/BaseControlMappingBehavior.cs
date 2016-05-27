using System.Linq;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using StructureMap;

namespace TerminalBase.Infrastructure.Behaviors
{
    public abstract class BaseControlMappingBehavior<T> where T : ControlDefinitionDTO
    {
        public const string ConfigurationControlsLabel = "Configuration_Controls";
        public static string BehaviorPrefix = "";

        protected ICrateManager _crateManager;
        protected ICrateStorage _crateStorage;
        protected string _behaviorName;
        
        protected BaseControlMappingBehavior(ICrateStorage crateStorage,string behaviorName)
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
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
                var crate = _crateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
                _crateStorage.Add(crate);

                controlsCM = crate.Content;
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
