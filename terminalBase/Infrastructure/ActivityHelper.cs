using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Helpers;
using Fr8Data.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;

namespace TerminalBase.Infrastructure
{
    public static class ActivityHelper
    {
        /// <summary>
        /// Lets you update activity UI control values without need to unpack and repack control crates
        /// </summary>
        public static ActivityDO UpdateControls<TActivityUi>(this ActivityDO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            return UpdateControls((object)activity, action) as ActivityDO;
        }

        /// <summary>
        /// Lets you update activity UI control values without need to unpack and repack control crates
        /// </summary>
        public static ActivityDTO UpdateControls<TActivityUi>(this ActivityDTO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            return UpdateControls((object)activity, action) as ActivityDTO;
        }

        private static object UpdateControls<TActivityUi>(object activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var activityDO = activity as ActivityDO;
            var activityDTO = activity as ActivityDTO;
            var crateManager = new CrateManager();
            using (var storage = activityDO == null ? crateManager.GetUpdatableStorage(activityDTO) : crateManager.GetUpdatableStorage(activityDO))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
                activityUi.RestoreDynamicControlsFrom(controlsCrate.Content);
                action(activityUi);
                var newControls = new StandardConfigurationControlsCM(activityUi.Controls.ToArray());
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, newControls, controlsCrate.Availability));
                activityUi.SaveDynamicControlsTo(newControls);

            }
            return activityDO ?? (object)activityDTO;
        }

        /// <summary>
        /// Returns a copy of AcvitityUI for the given activity
        /// </summary>
        public static TActivityUi GetReadonlyActivityUi<TActivityUi>(this ActivityDO activity) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            var crateManager = new CrateManager();
            var storage = crateManager.GetStorage(activity);
            var controls = storage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content;
            var activityUi =  new TActivityUi().ClonePropertiesFrom(controls) as TActivityUi;
            activityUi.RestoreDynamicControlsFrom(controls);
            return activityUi;
        }

        internal static void RestoreDynamicControlsFrom<TActivityUi>(this TActivityUi source, StandardConfigurationControlsCM destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var dynamicControlsCollection = Fr8ReflectionHelper.GetMembers(source.GetType()).Where(x => x.CanRead && x.GetCustomAttribute<DynamicControlsAttribute>() != null && CheckIfMemberIsControlsCollection(x)).ToDictionary(x => x.Name, x => x);

            if (dynamicControlsCollection.Count > 0)
            {
                foreach (var control in destination.Controls)
                {
                    if (string.IsNullOrWhiteSpace(control.Name))
                    {
                        continue;
                    }

                    var delim = control.Name.IndexOf('_');

                    if (delim <= 0)
                    {
                        continue;
                    }

                    var prefix = control.Name.Substring(0, delim);
                    IMemberAccessor member;

                    if (!dynamicControlsCollection.TryGetValue(prefix, out member))
                    {
                        continue;
                    }

                    var controlsCollection = (IList)member.GetValue(source);

                    if (controlsCollection == null && (!member.CanWrite || member.MemberType.IsAbstract || member.MemberType.IsInterface))
                    {
                        continue;
                    }

                    if (controlsCollection == null)
                    {
                        controlsCollection = (IList)Activator.CreateInstance(member.MemberType);
                        member.SetValue(source, controlsCollection);
                    }

                    control.Name = control.Name.Substring(delim + 1);
                    controlsCollection.Add(control);
                }
            }
        }

        internal static void SaveDynamicControlsTo<TActivityUi>(this TActivityUi source, StandardConfigurationControlsCM destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var insertIndex = 0;

            foreach (var member in Fr8ReflectionHelper.GetMembers(source.GetType()).Where(x => x.CanRead))
            {
                if (member.GetCustomAttribute<DynamicControlsAttribute>() != null && CheckIfMemberIsControlsCollection(member))
                {
                    var collection = member.GetValue(source) as IList;

                    if (collection != null)
                    {
                        foreach (var control in collection.Cast<object>().OfType<ControlDefinitionDTO>())
                        {
                            control.Name = member.Name + "_" + control.Name;
                            destination.Controls.Insert(insertIndex, control);
                            insertIndex++;
                        }
                    }
                }

                var controlDef = member.GetValue(source) as IControlDefinition;
                if (!string.IsNullOrWhiteSpace(controlDef?.Name))
                {
                    for (int i = 0; i < destination.Controls.Count; i++)
                    {
                        if (destination.Controls[i].Name == controlDef.Name)
                        {
                            insertIndex = i + 1;
                            break;
                        }
                    }
                }
            }
        }

        private static bool CheckIfMemberIsControlsCollection(IMemberAccessor member)
        {
            if (member.MemberType.IsInterface && CheckIfTypeIsControlsCollection(member.MemberType))
            {
                return true;
            }

            return member.MemberType.GetInterfaces().Any(CheckIfTypeIsControlsCollection);
        }

        private static bool CheckIfTypeIsControlsCollection(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();

                if (typeof(IList<>) == genericTypeDef)
                {
                    if (typeof(IControlDefinition).IsAssignableFrom(type.GetGenericArguments()[0]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
