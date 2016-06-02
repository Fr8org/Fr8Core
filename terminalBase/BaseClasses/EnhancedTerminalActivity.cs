using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Helpers;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace TerminalBase.BaseClasses
{
    public abstract class EnhancedTerminalActivity<T> : BaseTerminalActivityLegacy
       where T : StandardConfigurationControlsCM
    {
        /**********************************************************************************/
        
        private const string ConfigurationValuesCrateLabel = "Configuration Values";
        /// <summary>
        /// Get or sets value of configuration field with the given key stored in current activity storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string this[string key]
        {
            get
            {
                CheckCurrentActivityStorageAvailability();
                var crate = Storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationValuesCrateLabel);
                return crate?.Content.Fields.FirstOrDefault(x => x.Key == key)?.Value;
            }
            set
            {
                CheckCurrentActivityStorageAvailability();
                var crate = Storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationValuesCrateLabel);
                if (crate == null)
                {
                    crate = Crate<FieldDescriptionsCM>.FromContent(ConfigurationValuesCrateLabel, new FieldDescriptionsCM(), AvailabilityType.Configuration);
                    Storage.Add(crate);
                }
                var field = crate.Content.Fields.FirstOrDefault(x => x.Key == key);
                if (field == null)
                {
                    field = new FieldDTO(key, AvailabilityType.Configuration);
                    crate.Content.Fields.Add(field);
                }
                field.Value = value;
                Storage.ReplaceByLabel(crate);
            }
        }

        public T ActivityUI { get; private set; }

        protected UiBuilder UiBuilder { get; }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        protected EnhancedTerminalActivity(ICrateManager crateManager) 
            : base(crateManager)
        {
            UiBuilder = new UiBuilder();
        }

        /**********************************************************************************/

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();
            SyncConfControls(false);
        }

        /**********************************************************************************/

        protected override async Task<bool> BeforeConfigure(ConfigurationRequestType configurationRequestType)
        {
            if (configurationRequestType == ConfigurationRequestType.Initial)
            {
                Storage.Clear();
                Storage.Add(Crate.FromContent(ConfigurationControlsLabel, ActivityUI, AvailabilityType.Configuration));
            }

            return await base.BeforeConfigure(configurationRequestType);
        }

        /**********************************************************************************/

        protected override Task AfterConfigure(ConfigurationRequestType configurationRequestType, Exception ex)
        {
            SyncConfControlsBack();
            return base.AfterConfigure(configurationRequestType, ex);
        }

        /**********************************************************************************/

        protected override Task AfterActivate(Exception ex)
        {
            SyncConfControlsBack();
            return base.AfterActivate(ex);
        }

        /**********************************************************************************/

        protected override Task AfterDeactivate(Exception ex)
        {
            SyncConfControlsBack();
            return base.AfterDeactivate(ex);
        }
        
        /**********************************************************************************/

        protected T AssignNamesForUnnamedControls(T configurationControls)
        {
            int controlId = 0;
            var controls = configurationControls.EnumerateControlsDefinitions();

            foreach (var controlDefinition in controls)
            {
                if (string.IsNullOrWhiteSpace(controlDefinition.Name))
                {
                    controlDefinition.Name = controlDefinition.GetType().Name + controlId++;
                }
            }

            return configurationControls;
        }

        protected override ValidationManager CreateValidationManager()
        {
           return new EnhancedValidationManager<T>(this, IsRuntime ? Payload : null);
        }

        /**********************************************************************************/

        protected virtual T CrateActivityUI()
        {
            var uiBuilderConstructor = typeof(T).GetConstructor(new[] { typeof(UiBuilder) });

            if (uiBuilderConstructor != null)
            {
                return AssignNamesForUnnamedControls((T)uiBuilderConstructor.Invoke(new object[] { UiBuilder }));
            }

            var defaultConstructor = typeof(T).GetConstructor(new Type[0]);

            if (defaultConstructor == null)
            {
                throw new InvalidOperationException($"Unable to find default constructor or constructor accepting UiBuilder for type {typeof(T).FullName}");
            }

            return AssignNamesForUnnamedControls((T)defaultConstructor.Invoke(null));
        }
        
        /**********************************************************************************/

        private void CheckCurrentActivityStorageAvailability()
        {
            if (Storage == null)
            {
                throw new ApplicationException("Current activity storage is not available");
            }
        }

        /**********************************************************************************/
        // SyncConfControls and SyncConfControlsBack are pair of methods that serves the following reason:
        // We want to work with StandardConfigurationControlsCM in form of ActivityUi that has handy properties to directly access certain controls
        // But when we deserialize activity's crate storage we get StandardConfigurationControlsCM. So we need a way to 'convert' StandardConfigurationControlsCM
        // from crate storage to ActivityUI.
        // SyncConfControls takes properties of controls in StandardConfigurationControlsCM from activity's storage and copies them into ActivityUi.
        private void SyncConfControls(bool throwException)
        {
            var configurationControls = Storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            ActivityUI = CrateActivityUI();

            if (configurationControls == null)
            {
                if (throwException)
                {
                    throw new InvalidOperationException("Configuration controls crate is missing");
                }

                return;
            }

            ActivityUI.SyncWith(configurationControls);

            if (ActivityUI.Controls != null)
            {
                ActivityUI.RestoreDynamicControlsFrom(configurationControls);
            }
        }

        /**********************************************************************************/
        // Activity logic can update state of configuration controls. But as long we have copied  configuration controls crate from the storage into 
        // new instance of ActivityUi changes made to ActivityUi won't be reflected in storage.
        // we have to persist in storage changes we've made to ActivityUi.
        // we do this in the most simple way by replacing old StandardConfigurationControlsCM with ActivityUi.
        private void SyncConfControlsBack()
        {
            Storage.Remove<StandardConfigurationControlsCM>();
            // we create new StandardConfigurationControlsCM with controls from ActivityUi.
            // We do this because ActivityUi can has properties to access specific controls. We don't want those propeties exist in serialized crate.

            var configurationControlsToAdd = new StandardConfigurationControlsCM(ActivityUI.Controls);
            Storage.Add(Crate.FromContent(ConfigurationControlsLabel, configurationControlsToAdd, AvailabilityType.Configuration));
            ActivityUI.SaveDynamicControlsTo(configurationControlsToAdd);
        }
    }
}
