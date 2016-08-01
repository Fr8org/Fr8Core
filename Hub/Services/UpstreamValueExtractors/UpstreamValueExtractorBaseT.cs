using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Manifests;

namespace Hub.Services.UpstreamValueExtractors
{
    public abstract class UpstreamValueExtractorBase<T> : IUpstreamValueExtractor
    {
        public Type ConfigurationControlType => typeof(T);

        public void ExtractUpstreamValue(object configurationControl, ICrateStorage crateStorage)
        {
            var control = (T) configurationControl;
            
            ExtractUpstreamValue(control, crateStorage);
        }

        protected abstract void ExtractUpstreamValue(T configurationControl, ICrateStorage crateStorage);
        
        protected static object GetValue(ICrateStorage payloadStorage, FieldDTO fieldToMatch)
        {
            if (payloadStorage == null)
            {
                throw new ArgumentNullException(nameof(payloadStorage));
            }

            if (fieldToMatch == null)
            {
                throw new ArgumentNullException(nameof(fieldToMatch));
            }

            if (string.IsNullOrWhiteSpace(fieldToMatch.Name))
            {
                return null;
            }

            IEnumerable<Crate> filteredCrates = payloadStorage;

            if (!string.IsNullOrWhiteSpace(fieldToMatch.SourceActivityId))
            {
                filteredCrates = filteredCrates.Where(x => x.SourceActivityId == fieldToMatch.SourceActivityId);
            }
            if (!string.IsNullOrEmpty(fieldToMatch.SourceCrateLabel))
            {
                filteredCrates = filteredCrates.Where(x => x.Label == fieldToMatch.SourceCrateLabel);
            }
            if (fieldToMatch.SourceCrateManifest != CrateManifestType.Any && fieldToMatch.SourceCrateManifest != CrateManifestType.Unknown)
            {
                filteredCrates = filteredCrates.Where(x => x.ManifestType.Equals(fieldToMatch.SourceCrateManifest));
            }

            //iterate through found crates and search for the field with the specified key
            foreach (var crate in filteredCrates)
            {
                // skip system crates
                if (crate.IsOfType<OperationalStateCM>() || crate.IsOfType<CrateDescriptionCM>() || crate.IsOfType<ValidationResultsCM>())
                {
                    continue;
                }

                var foundValue = GetValue(crate, fieldToMatch.Name);

                if (foundValue != null)
                {
                    return foundValue;
                }
            }

            return null;
        }


        private static object GetValue(Crate crate, string fieldKey)
        {
            if (crate.IsOfType<StandardTableDataCM>())
            {
                var tableCrate = crate.Get<StandardTableDataCM>();
                if (tableCrate.FirstRowHeaders && tableCrate.Table.Count > 1)
                {
                    return tableCrate.Table[1].Row.FirstOrDefault(a => a.Cell.Key == fieldKey)?.Cell?.Value;
                }
            }
            
            if (crate.IsKnownManifest)
            {
                var data = crate.Get();
                object value = null;

                Fr8ReflectionHelper.VisitPropertiesRecursive(data, (instance, member) =>
                {
                    if (!member.CanRead)
                    {
                        return Fr8ReflectionHelper.PropertiesVisitorOp.Continue;
                    }

                    var manifestAttr = member.GetCustomAttribute<ManifestFieldAttribute>();

                    if (manifestAttr != null && manifestAttr.IsHidden)
                    {
                        return Fr8ReflectionHelper.PropertiesVisitorOp.Continue;
                    }

                    var tempValue = member.GetValue(instance);

                    if (member.Name == fieldKey)
                    {
                        value = tempValue;
                        return Fr8ReflectionHelper.PropertiesVisitorOp.Terminate;
                    }

                    var keyValuePair = tempValue as KeyValueDTO;
                    if (keyValuePair != null)
                    {
                        if (keyValuePair.Key == fieldKey)
                        {
                            value = keyValuePair.Value;
                            return Fr8ReflectionHelper.PropertiesVisitorOp.Terminate;
                        }

                        return Fr8ReflectionHelper.PropertiesVisitorOp.SkipBranch;

                    }

                    return Fr8ReflectionHelper.PropertiesVisitorOp.Continue;
                });

                return value;
            }

            // do nothing for uknown manifests
            return null;
        }
    }
}
