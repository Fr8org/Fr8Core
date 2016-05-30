using System;
using System.Linq;
using System.Linq.Expressions;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace Fr8Data.Helpers
{
    public static class Fr8ApiHelper
    {
        public static string FindField(this ICrateStorage payloadStorage, string fieldKey, bool ignoreCase = false, MT? manifestType = null, string label = null)
        {
            //search through every crate except operational state crate
            Expression<Func<Crate, bool>> defaultSearchArguments = (c) => c.ManifestType.Id != (int)MT.OperationalStatus;

            //apply label criteria if not null
            if (label != null)
            {
                Expression<Func<Crate, bool>> andLabel = (c) => c.Label == label;
                defaultSearchArguments = Expression.Lambda<Func<Crate, bool>>(Expression.AndAlso(defaultSearchArguments, andLabel), defaultSearchArguments.Parameters);
            }

            //apply manifest criteria if not null
            if (manifestType != null)
            {
                Expression<Func<Crate, bool>> andManifestType = (c) => c.ManifestType.Id == (int)manifestType;
                defaultSearchArguments = Expression.Lambda<Func<Crate, bool>>(Expression.AndAlso(defaultSearchArguments, andManifestType), defaultSearchArguments.Parameters);
            }

            //find user requested crate
            var foundCrates = payloadStorage.Where(defaultSearchArguments.Compile()).ToList();
            if (!foundCrates.Any())
            {
                return null;
            }

            //get operational state crate to check for loops
            var operationalState = payloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            //iterate through found crates to find the payload
            foreach (var foundCrate in foundCrates)
            {
                var foundField = FindField(operationalState, foundCrate, fieldKey);
                if (foundField != null)
                {
                    return foundField.Value;
                }
            }

            return null;
        }

        public static string FindField(this ICrateStorage payloadStorage, FieldDTO fieldToMatch)
        {
            if (payloadStorage == null)
            {
                throw new ArgumentNullException(nameof(payloadStorage));
            }
            if (fieldToMatch == null)
            {
                throw new ArgumentNullException(nameof(fieldToMatch));
            }
            if (string.IsNullOrWhiteSpace(fieldToMatch.Key))
            {
                return null;
            }
            var filteredCrates = payloadStorage.AsQueryable();
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
            var operationalState = payloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            //iterate through found crates to find the payload
            foreach (var foundCrate in filteredCrates)
            {
                var foundField = FindField(operationalState, foundCrate, fieldToMatch.Key);
                if (foundField != null)
                {
                    return foundField.Value;
                }
            }
            return null;
        }


        private static FieldDTO FindField(OperationalStateCM operationalState, Crate crate, string fieldKey)
        {
            object searchArea;
            //let's check if we are in a loop
            //and this is a loop data?
            //check if this crate is loop related
            var loopState = operationalState.CallStack.FirstOrDefault(x =>
            {
                if (x.LocalData?.Type == "Loop")
                {
                    var loopStatus = x.LocalData.ReadAs<OperationalStateCM.LoopStatus>();

                    if (loopStatus != null && loopStatus.CrateManifest.CrateDescriptions[0].Label == crate.Label && loopStatus.CrateManifest.CrateDescriptions[0].ManifestType == crate.ManifestType.Type)
                    {
                        return true;
                    }
                }

                return false;
            });

            if (loopState != null) //this is a loop related data request
            {
                searchArea = GetDataListItem(crate, loopState.LocalData.ReadAs<OperationalStateCM.LoopStatus>().Index);
            }
            else
            {
                //hmmm this is a regular data request
                //lets search in complete crate
                searchArea = crate;
                //if we have a StandardTableDataCM and we are not in the loop and crate has Headers - we should search next row
                if (crate.IsOfType<StandardTableDataCM>())
                {
                    var tableCrate = crate.Get<StandardTableDataCM>();
                    if (tableCrate.FirstRowHeaders && tableCrate.Table.Count > 1)
                    {
                        //TODO it is weird to get just first row of table data while searching for a field
                        //note: GetDataListItem function skips header
                        TableRowDTO row = GetDataListItem(crate, 0) as TableRowDTO;
                        if (row != null)
                            return row.Row.FirstOrDefault(a => a.Cell.Key == fieldKey)?.Cell;
                    }
                }
            }

            if (searchArea is Crate)
            {
                if (((Crate) searchArea).IsKnownManifest)
                {
                    searchArea = ((Crate) searchArea).Get();
                }
                else
                {
                    return null;
                }
            }

            //we should find first related field and return
            var fields = Fr8ReflectionHelper.FindFieldsRecursive(searchArea);
            var fieldMatch = fields.FirstOrDefault(f => f.Key == fieldKey);
            //let's return first match
            return fieldMatch;
        }

        private static object GetDataListItem(Crate crate, int index)
        {
            var tableData = crate.ManifestType.Id == (int)MT.StandardTableData ? crate.Get<StandardTableDataCM>() : null;
            if (tableData != null)
            {
                //why?? why just skip header and return first row?
                return tableData.FirstRowHeaders ? tableData.Table[index + 1] : tableData.Table[index];
            }
            return Fr8ReflectionHelper.FindFirstArray(crate.Get())[index];
        }
    }
}
