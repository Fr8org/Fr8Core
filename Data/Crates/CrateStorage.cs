using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Data.Crates
{
    [JsonConverter(typeof(DenySerializationConverter), "CrateStorage can't be directly serialized to JSON. Convert it to CrateStorageDTO.")]
    public class CrateStorage : IEnumerable<Crate>
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly Dictionary<string, Crate> _crates = new Dictionary<string, Crate>();

        /**********************************************************************************/

        public int Count
        {
            get { return _crates.Count; }
        }

        /**********************************************************************************/
        /// <summary>
        /// Get crate by id.
        /// Property getter will fail if there is no such crate.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Crate this[string key]
        {
            get { return _crates[key]; }
            set { _crates[key] = value; }
        }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public CrateStorage()
        {
        }

        /**********************************************************************************/

        public CrateStorage(params Crate[] crates)
            : this((IEnumerable<Crate>)crates)
        {
        }

        /**********************************************************************************/

        public CrateStorage(IEnumerable<Crate> crates)
        {
            foreach (var crate in crates)
            {
                _crates.Add(crate.Id, crate);
            }
        }

        /**********************************************************************************/
        /// <summary>
        /// Add new crate to storage
        /// </summary>
        /// <param name="crate"></param>
        public void Add(Crate crate)
        {
            _crates.Add(crate.Id, crate);
        }

        /**********************************************************************************/
        /// <summary>
        /// Add collection of crates to storage
        /// </summary>
        /// <param name="crates"></param>
        public void AddRange(IEnumerable<Crate> crates)
        {
            foreach (var crate in crates)
            {
                Add(crate);
            }
        }

        /**********************************************************************************/
        /// <summary>
        /// Removes all crates from storage
        /// </summary>
        public void Clear()
        {
            _crates.Clear();
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns first crate that complies with the predicate and with content of the give type.
        /// This method will fail if no such crates exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Crate<T> FirstCrate<T>(Predicate<Crate> predicate)
        {
            return CratesOfType<T>(predicate).First();
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns first crate that complies with the predicate and with content of the give type.
        /// This method will return NULL if no such crates exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Crate<T> FirstCrateOrDefault<T>(Predicate<Crate> predicate)
        {
            return CratesOfType<T>(predicate).FirstOrDefault();
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates with content of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Crate<T>> CratesOfType<T>()
        {
            return CratesOfType<T>(null);
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates that complies with the predicate and with content of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<Crate<T>> CratesOfType<T>(Predicate<Crate> predicate)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                yield break;
            }

            foreach (var crate in _crates.Values)
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
        public IEnumerable<T> CrateContentsOfType<T>()
        {
            return CratesOfType<T>().Select(x => x.Get<T>());
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns all crates content that complies with the predicate and with content of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<T> CrateContentsOfType<T>(Predicate<Crate> predicate)
        {
            return CratesOfType<T>().Where(x => predicate(x)).Select(x => x.Get<T>());
        }

        /**********************************************************************************/
        /// <summary>
        /// Find content of given type among all crates in the storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="crateContent"></param>
        /// <returns></returns>
        public bool TryGetValue<T>(out T crateContent)
        {
            return TryGetValue(x => true, out crateContent);
        }

        /**********************************************************************************/
        /// <summary>
        /// Find content of given type among all crates that complies with the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="crateContent"></param>
        /// <returns></returns>
        public bool TryGetValue<T>(Predicate<Crate> predicate, out T crateContent)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                crateContent = default(T);
                return false;
            }

            foreach (var crate in _crates.Values)
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
        public int RemoveUsingPredicate(Predicate<Crate> predicate)
        {
            int affectedItems = 0;

            foreach (var key in _crates.Keys.ToArray())
            {
                if (predicate(_crates[key]))
                {
                    affectedItems++;
                    _crates.Remove(key);
                }
            }

            return affectedItems;
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates with the content of given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Remove<T>()
        {
            CrateManifestType manifestType;
            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                return 0;
            }

            int removed = 0;

            foreach (var source in _crates.Values.ToArray())
            {
                if (source.ManifestType == manifestType)
                {
                    _crates.Remove(source.Id);
                    removed++;
                }
            }

            return removed;
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates by the manifest id
        /// </summary>
        /// <param name="manifestId"></param>
        /// <returns></returns>
        public int RemoveByManifestId(int manifestId)
        {
            return RemoveUsingPredicate(x => x.ManifestType.Id == manifestId);
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove all crates by the label
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public int RemoveByLabel(string label)
        {

            return RemoveUsingPredicate(x => x.Label == label);
        }

        /**********************************************************************************/
        /// <summary>
        /// Replaces all crates that have label mathching to passed crate label with passed crate 
        /// </summary>
        /// <returns></returns>
        public int ReplaceByLabel(Crate crate)
        {
            int affected_items = 0;
            var predicate = new Predicate<Crate>(x => x.Label == crate.Label);

            foreach (var key in _crates.Keys.ToArray())
            {
                if (predicate(_crates[key]))
                {
                    _crates.Remove(key);
                    affected_items++;
                }
            }

            this.Add(crate);
            return affected_items;
        }

        /**********************************************************************************/
        /// <summary>
        /// Checks if there is a crate with the given id
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _crates.ContainsKey(key);
        }

        /**********************************************************************************/
        /// <summary>
        /// Removes the crate. Crate is removed by Id.
        /// </summary>
        /// <param name="crate"></param>
        /// <returns></returns>
        public bool Remove(Crate crate)
        {
            return Remove(crate.Id);
        }

        /**********************************************************************************/
        /// <summary>
        /// Remove crate by Id
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return _crates.Remove(key);
        }

        /**********************************************************************************/
        /// <summary>
        /// Try to get crate by id
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out Crate value)
        {
            return _crates.TryGetValue(key, out value);
        }

        /**********************************************************************************/

        public IEnumerator<Crate> GetEnumerator()
        {
            return _crates.Values.GetEnumerator();
        }

        /**********************************************************************************/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_crates).GetEnumerator();
        }

        /**********************************************************************************/

        public CrateManifestType GetManifestType<T>()
        {
            CrateManifestType manifestType;
            ManifestTypeCache.TryResolveManifest(typeof (T), out manifestType);
            return manifestType;
        }
    }
}
