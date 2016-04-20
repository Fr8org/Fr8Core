using Data.Constants;
using Data.Helpers;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Crates
{
    public static class CrateStorageExtensions
    {
        /**********************************************************************************/
        /// <summary>
        /// Add collection of crates to storage
        /// </summary>
        /// <param name="crates"></param>
        public static void AddRange(this ICrateStorage storage, IEnumerable<Crate> crates)
        {
            foreach (var crate in crates)
            {
                storage.Add(crate);
            }
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns first crate that complies with the predicate and with content of the given type.
        /// This method will fail if no such crates exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Crate<T> FirstCrate<T>(this ICrateStorage storage, Predicate<Crate> predicate)
        {
            return storage.CratesOfType<T>(predicate).First();
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns first crate of the given type.
        /// This method will fail if no such crates exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Crate<T> FirstCrate<T>(this ICrateStorage storage)
        {
            return storage.CratesOfType<T>().First();
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns first crate that complies with the predicate and with content of the given type.
        /// This method will return NULL if no such crates exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Crate<T> FirstCrateOrDefault<T>(this ICrateStorage storage, Predicate<Crate> predicate)
        {
            return storage.CratesOfType<T>(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Returns first crate with content of the given type.
        /// This method will return NULL if no such crates exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Crate<T> FirstCrateOrDefault<T>(this ICrateStorage storage)
        {
            return storage.CratesOfType<T>().FirstOrDefault();
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates with content of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Crate<T>> CratesOfType<T>(this ICrateStorage storage)
        {
            return storage.CratesOfType<T>(null);
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates that complies with the predicate and with content of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<Crate<T>> CratesOfType<T>(this ICrateStorage storage, Predicate<Crate> predicate)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                yield break;
            }

            foreach (var crate in storage)
            {
                if (crate.ManifestType == manifestType && (predicate == null || predicate(crate)))
                {
                    yield return new Crate<T>(crate);
                }
            }
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates content of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> CrateContentsOfType<T>(this ICrateStorage storage)
        {
            return storage.CratesOfType<T>().Select(x => x.Get<T>());
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates content that complies with the predicate and with content of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> CrateContentsOfType<T>(this ICrateStorage storage, Predicate<Crate> predicate)
        {
            return storage.CratesOfType<T>().Where(x => predicate(x)).Select(x => x.Get<T>());
        }

        /**********************************************************************************/
        /// <summary>
        /// Find content of given type among all crates in the storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="crateContent"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this ICrateStorage storage, out T crateContent)
        {
            return storage.TryGetValue(x => true, out crateContent);
        }

        /**********************************************************************************/
        /// <summary>
        /// Find content of given type among all crates that complies with the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="crateContent"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this ICrateStorage storage, Predicate<Crate> predicate, out T crateContent)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                crateContent = default(T);
                return false;
            }

            foreach (var crate in storage)
            {
                if (crate.ManifestType == manifestType && predicate(crate))
                {
                    crateContent = crate.Get<T>();
                    return true;
                }
            }

            crateContent = default(T);
            return false;
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates that complies with the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int RemoveUsingPredicate(this ICrateStorage storage, Predicate<Crate> predicate)
        {
            return storage.Remove(predicate);
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates with the content of given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Remove<T>(this ICrateStorage storage)
        {
            CrateManifestType manifestType;
            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                return 0;
            }

            return storage.Remove(x => x.ManifestType == manifestType);
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates by the manifest id
        /// </summary>
        /// <param name="manifestId"></param>
        /// <returns></returns>
        public static int RemoveByManifestId(this ICrateStorage storage, int manifestId)
        {
            return storage.Remove(x => x.ManifestType.Id == manifestId);
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates by the label
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static int RemoveByLabel(this ICrateStorage storage, string label)
        {
            return storage.Remove(x => x.Label == label);
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates by the label prefix
        /// </summary>
        /// <param name="labelPrefix">E.g. "Data from: " that is used in Get_Google_Sheet_Data</param>
        /// <returns></returns>
        public static int RemoveByLabelPrefix(this ICrateStorage storage, string labelPrefix)
        {
            return storage.Remove(x => x.Label.StartsWith(labelPrefix));
        }

        /**********************************************************************************/
        /// <summary>
        /// Replaces all crates that have label mathching to passed crate label with passed crate 
        /// </summary>
        /// <returns></returns>
        public static int ReplaceByLabel(this ICrateStorage storage, Crate crate)
        {
            var predicate = new Predicate<Crate>(x => x.Label == crate.Label);

            return storage.Replace(predicate, crate);
        }

        /**********************************************************************************/
        /// <summary>
        /// Removes the crate. Crate is removed by Id.
        /// </summary>
        /// <param name="crate"></param>
        /// <returns></returns>
        public static bool Remove(this ICrateStorage storage, Crate crate)
        {
            return storage.Remove(x => x.Id == crate.Id) != 0;
        }
        /// <summary>
        /// Retrieves the field value from storage for the specified key
        /// </summary>
        /// <param name="payloadStorage"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="manifestType"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static string RetrieveValue(this ICrateStorage payloadStorage, string key, bool ignoreCase = false, MT? manifestType = null, string label = null)
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
                var foundField = FindField(key, operationalState, foundCrate);
                if (foundField != null)
                {
                    return foundField.Value;
                }
            }

            return null;
        }

        private static FieldDTO FindField(string key, OperationalStateCM operationalState, Crate crate)
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

                    if (loopStatus != null && loopStatus.Label == crate.Label && loopStatus.CrateManifest == crate.ManifestType.Type)
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
                    var table_crate = crate.Get<StandardTableDataCM>();
                    if (table_crate.FirstRowHeaders && table_crate.Table.Count > 1)
                    {
                        TableRowDTO row = GetDataListItem(crate, 0) as TableRowDTO;
                        if (row != null)
                        {
                            return row.Row.Where(a => a.Cell.Key == key).FirstOrDefault().Cell;
                        }
                    }
                }
            }

            //we should find first related field and return
            var fields = Fr8ReflectionHelper.FindFieldsRecursive(searchArea);
            var fieldMatch = fields.FirstOrDefault(f => f.Key == key);
            //let's return first match
            return fieldMatch;
        }

        private static object GetDataListItem(Crate crate, int index)
        {
            var tableData = crate.ManifestType.Id == (int)MT.StandardTableData ? crate.Get<StandardTableDataCM>() : null;
            if (tableData != null)
            {
                return tableData.FirstRowHeaders ? tableData.Table[index + 1] : tableData.Table[index];
            }
            return Fr8ReflectionHelper.FindFirstArray(crate.Get())[index];
        }
    }
}