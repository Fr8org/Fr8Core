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

        public void Add(Crate crate)
        {
            _crates.Add(crate.Id, crate);
        }
        
        /**********************************************************************************/

        public void AddRange(IEnumerable<Crate> crates)
        {
            foreach (var crate in crates)
            {
                Add(crate);
            }
        }

        /**********************************************************************************/

        public void Clear()
        {
            _crates.Clear();
        }
        
        /**********************************************************************************/

        public Crate<T> FirstCrate<T>(Predicate<Crate> predicate)
        {
            return CratesOfType<T>(predicate).First();
        }

        /**********************************************************************************/

        public Crate<T> FirstCrateOrDefault<T>(Predicate<Crate> predicate)
        {
            return CratesOfType<T>(predicate).FirstOrDefault();
        }

        /**********************************************************************************/

        public IEnumerable<Crate<T>> CratesOfType<T>()
        {
            return CratesOfType<T>(null);
        }

        /**********************************************************************************/

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

        public IEnumerable<T> CrateContentsOfType<T>()
        {
            return CratesOfType<T>().Select(x => x.Get<T>());
        }

        /**********************************************************************************/

        public IEnumerable<T> CrateContentsOfType<T>(Predicate<Crate> predicate)
        {
            return CratesOfType<T>().Where(x => predicate(x)).Select(x => x.Get<T>());
        }

        /**********************************************************************************/

        public bool TryGetValue<T>(out T crateContent)
        {
            return TryGetValue(x => true, out crateContent);
        }

        /**********************************************************************************/

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

        public int RemoveByManifestId(int manifestId)
        {
            return RemoveUsingPredicate(x => x.ManifestType.Id == manifestId);
        }

        /**********************************************************************************/

        public int RemoveByLabel(string label)
        {
            return RemoveUsingPredicate(x => x.Label == label);
        }

        /**********************************************************************************/
        
        public bool ContainsKey(string key)
        {
            return _crates.ContainsKey(key);
        }

        /**********************************************************************************/

        public bool Remove(Crate crate)
        {
            return Remove(crate.Id);
        }

        /**********************************************************************************/

        public bool Remove(string key)
        {
            return _crates.Remove(key);
        }

        /**********************************************************************************/

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
    }
}
