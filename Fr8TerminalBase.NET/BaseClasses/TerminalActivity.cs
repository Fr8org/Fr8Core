using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Infrastructure.States;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    /// <summary>
    /// Recommended base class for developing new activities.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/TerminalActivityT.md
    /// </summary>
    /// <typeparam name="TUi"></typeparam>
    public abstract class TerminalActivity<TUi> : TerminalActivityBase
       where TUi : StandardConfigurationControlsCM
    {
        /**********************************************************************************/

        public TUi ActivityUI { get; private set; }
        
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        protected TerminalActivity(ICrateManager crateManager) 
            : base(crateManager)
        {
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
                Storage.Add(Crate.FromContent(ConfigurationControlsLabel, ActivityUI));
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

        private TUi AssignNamesForUnnamedControls(TUi configurationControls)
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

        /**********************************************************************************/

        protected override ValidationManager CreateValidationManager()
        {
           return new EnhancedValidationManager<TUi>(this, IsRuntime ? Payload : null);
        }

        /**********************************************************************************/

        private TUi CreateActivityUi()
        {
            var uiBuilderConstructor = typeof(TUi).GetConstructor(new[] { typeof(UiBuilder) });

            if (uiBuilderConstructor != null)
            {
                return AssignNamesForUnnamedControls((TUi)uiBuilderConstructor.Invoke(new object[] { UiBuilder }));
            }

            var defaultConstructor = typeof(TUi).GetConstructor(new Type[0]);

            if (defaultConstructor == null)
            {
                throw new InvalidOperationException($"Unable to find default constructor or constructor accepting UiBuilder for type {typeof(TUi).FullName}");
            }

            return AssignNamesForUnnamedControls((TUi)defaultConstructor.Invoke(null));
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
            ActivityUI = CreateActivityUi();

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
            Storage.Add(Crate.FromContent(ConfigurationControlsLabel, configurationControlsToAdd));
            ActivityUI.SaveDynamicControlsTo(configurationControlsToAdd);
        }
    }
}
