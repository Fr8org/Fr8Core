using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Crates
{
    [JsonConverter(typeof(DenySerializationConverter), "CrateStorage can't be directly serialized to JSON. Convert it to CrateStorageDTO.")]
    public class CrateStorage : ICrateStorage
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

        public void Add(params Crate[] crates)
        {
            foreach (var crate in crates)
            {
                _crates.Add(crate.Id, crate);
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
        /// Remove all crates that complies with the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int Remove(Predicate<Crate> predicate)
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
        /// Replaces all crates that have label mathching to passed predicate with passed crate 
        /// </summary>
        /// <returns></returns>
        public int Replace(Predicate<Crate> predicate, Crate crate)
        {
            int affected_items = 0;

            foreach (var key in _crates.Keys.ToArray())
            {
                if (predicate(_crates[key]))
                {
                    _crates.Remove(key);
                    affected_items++;
                }
            }

            Add(crate);
            return affected_items;
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

        /**********************************************************************************/
    }
}
