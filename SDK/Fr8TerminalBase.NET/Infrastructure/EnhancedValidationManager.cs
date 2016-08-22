using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Helpers;

namespace Fr8.TerminalBase.Infrastructure
{
    public class EnhancedValidationManager<TActivityUi> : ValidationManager where TActivityUi : StandardConfigurationControlsCM
    {
        private readonly TActivityUi _activityUi;
        //'Owner' in this context is the name of property holding the collection that contains particular control
        private readonly Dictionary<IControlDefinition, string> _ownerNameByControl;


        public EnhancedValidationManager(TerminalActivity<TActivityUi> activity,  ICrateStorage payload) 
            : base(payload)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            _activityUi = activity.ActivityUI;
            _ownerNameByControl = new Dictionary<IControlDefinition, string>();

            FillOwnerByControl();
        }

        private void FillOwnerByControl()
        {
            foreach (var collectionProperty in Fr8ReflectionHelper.GetMembers(typeof(TActivityUi)).Where(x => x.CanRead
                                                                                                      && x.GetCustomAttribute<DynamicControlsAttribute>() != null
                                                                                                      && Fr8ReflectionHelper.CheckIfMemberIsCollectionOf<IControlDefinition>(x)))
            {
                var collection = collectionProperty.GetValue(_activityUi) as IEnumerable<IControlDefinition>;
                if (collection == null)
                {
                    continue;
                }
                foreach (var control in collection)
                {
                    _ownerNameByControl.Add(control, collectionProperty.Name);
                }
            }
        }

        protected override string ResolveControlName(ControlDefinitionDTO control)
        {
            string ownerName;
            return _ownerNameByControl.TryGetValue(control, out ownerName)
                       ? ActivityHelper.GetDynamicControlName(control.Name, ownerName)
                       : base.ResolveControlName(control);
        }
    }
}