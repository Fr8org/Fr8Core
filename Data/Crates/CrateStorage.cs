using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Data.Crates
{
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

        public ICollection<Crate> Crates
        {
            get { return _crates.Values; }
        }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public IEnumerator<Crate> GetEnumerator()
        {
            return _crates.Values.GetEnumerator();
        }

        /**********************************************************************************/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _crates).GetEnumerator();
        }

        /**********************************************************************************/

        public void Add(Crate crate)
        {
            _crates.Add(crate.Id, crate);
        }

        /**********************************************************************************/

        public void Add(string label, object content)
        {
            var crate = Crate.FromContent(content);
            
            crate.Label = label;

            Add(crate);
        }

        /**********************************************************************************/

        public void Clear()
        {
            _crates.Clear();
        }
        
        /**********************************************************************************/

        public IEnumerable<Crate<T>> CratesOfType<T>()
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(typeof(T), out manifestType))
            {
                yield break;
            }

            foreach (var crate in _crates.Values)
            {
                if (crate.ManifestType == manifestType)
                {
                    yield return new Crate<T>(crate);
                }
            }
        }

        /**********************************************************************************/

        public IEnumerable<T> CrateValuesOfType<T>()
        {
            return CratesOfType<T>().Select(x => x.Get<T>());
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
    }
}
