using System;
using System.Collections;
using System.Linq;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Helpers;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;
using TerminalBase.Models;

namespace TerminalBase.Helpers
{
    public static class ActivityHelper
    {
        /// <summary>
        /// Lets you update activity UI control values without need to unpack and repack control crates
        /// </summary>
        public static ActivityDTO UpdateControls<TActivityUi>(this ActivityDTO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var crateManager = new CrateManager();
            using (var storage = crateManager.GetUpdatableStorage(activity))
            {
                UpdateControls<TActivityUi>(storage, action);
            }

            return activity;
        }

        public static void UpdateControls<TActivityUi>(this ICrateStorage storage, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
            var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
            activityUi.RestoreDynamicControlsFrom(controlsCrate.Content);
            action(activityUi);
            var newControls = new StandardConfigurationControlsCM(activityUi.Controls.ToArray());
            storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, newControls));
            activityUi.SaveDynamicControlsTo(newControls);
        }

        internal static string GetDynamicControlName(string controlName, string controlOwnerName)
        {
            return $"{controlOwnerName}_{controlName}";
        }

        private static readonly Tuple<string, string> EmptyDynamicControlNameAndOwner = new Tuple<string, string>(String.Empty, String.Empty);

        internal static Tuple<string, string> GetDynamicControlNameAndOwner(string dynamicControlName)
        {
            if (String.IsNullOrWhiteSpace(dynamicControlName))
            {
                return EmptyDynamicControlNameAndOwner;
            }
            var delim = dynamicControlName.IndexOf('_');
            if (delim <= 0)
            {
                return EmptyDynamicControlNameAndOwner;
            }
            return new Tuple<string, string>(dynamicControlName.Substring(delim + 1), dynamicControlName.Substring(0, delim));
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
            var dynamicControlsCollection = Fr8ReflectionHelper.GetMembers(source.GetType()).Where(x => x.CanRead
                                                                                                        && x.GetCustomAttribute<DynamicControlsAttribute>() != null
                                                                                                        && Fr8ReflectionHelper.CheckIfMemberIsCollectionOf<IControlDefinition>(x))
                                                               .ToDictionary(x => x.Name, x => x);

            if (dynamicControlsCollection.Count > 0)
            {
                foreach (var control in destination.Controls)
                {
                    var nameAndOwner = GetDynamicControlNameAndOwner(control.Name);
                    if (String.IsNullOrEmpty(nameAndOwner.Item2))
                    {
                        continue;
                    }
                    IMemberAccessor member;

                    if (!dynamicControlsCollection.TryGetValue(nameAndOwner.Item2, out member))
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

                    control.Name = nameAndOwner.Item1;
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
                if (member.GetCustomAttribute<DynamicControlsAttribute>() != null && Fr8ReflectionHelper.CheckIfMemberIsCollectionOf<IControlDefinition>(member))
                {
                    var collection = member.GetValue(source) as IList;

                    if (collection != null)
                    {
                        foreach (var control in collection.Cast<object>().OfType<ControlDefinitionDTO>())
                        {
                            control.Name = GetDynamicControlName(control.Name, member.Name);
                            destination.Controls.Insert(insertIndex, control);
                            insertIndex++;
                        }
                    }
                }

                var controlDef = member.GetValue(source) as IControlDefinition;
                if (!String.IsNullOrWhiteSpace(controlDef?.Name))
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

        /// <summary>
        /// Returns a copy of AcvitityUI for the given activity
        /// </summary>
        public static TActivityUi GetReadonlyActivityUi<TActivityUi>(this ICrateStorage storage) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            var controls = storage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content;
            var activityUi = new TActivityUi().ClonePropertiesFrom(controls) as TActivityUi;
            activityUi.RestoreDynamicControlsFrom(controls);
            return activityUi;
        }
    }
}