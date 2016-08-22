using System;
using System.Collections.Generic;
using System.Linq;

namespace Fr8Data.Crates
{
    public static class CrateStorageExtensions
    {
        /**********************************************************************************/

        public static Crate<T> Add<T>(this ICrateStorage storage, string label, T content)
        {
            var crate = Crate.FromContent(label, content);

            storage.Add(crate);

            return crate;
        }

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
        /// Returns all crates content of the given type.
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
        /// Remove all crates with the content of given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAdd<T>(this ICrateStorage storage, Func<Crate<T>> createNewCrate)
        {
            var exising = storage.CrateContentsOfType<T>().FirstOrDefault();

            if (exising == null)
            {
                var newCrate = createNewCrate();

                storage.Add(newCrate);

                return newCrate.Content;
            }

            return exising;
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
    }
}