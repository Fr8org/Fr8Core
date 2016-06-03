using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace terminalGoogle.Infrastructure
{
    /// <summary> 
    /// Entity Framework implementation for storing dictionary form data in the database.
    /// </summary>
    /// <remarks>
    /// Two-step JSON serialization. Firstly, every value from pair (key-value) is serialized to string. Secondly, all pairs (key-serialized_value) are serialized to JSON.
    /// </remarks>
    public class JSONDataStore : IDataStore
    {
        private readonly Func<string> _getAccessor;
        private readonly Action<string> _setAccessor;

        /// <summary>
        /// Constructs JSONDataStore
        /// </summary>
        /// <param name="getAccessor">Function to get JSON string value.</param>
        /// <param name="setAccessor">Function to set JSON string value.</param>
        public JSONDataStore(Func<string> getAccessor, Action<string> setAccessor)
        {
            if (getAccessor == null)
                throw new ArgumentNullException("getAccessor");
            if (setAccessor == null)
                throw new ArgumentNullException("setAccessor");
            _getAccessor = getAccessor;
            _setAccessor = setAccessor;
        }

        private class StoreDictionary : IDictionary<string, string>
        {
            private readonly JSONDataStore _store;
            private IDictionary<string, string> _dictionary;

            private StoreDictionary(JSONDataStore store)
            {
                _store = store;
            }

            private void LoadDictionary()
            {
                var json = _store._getAccessor();
                _dictionary = string.IsNullOrEmpty(json)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }

            public static StoreDictionary GetStoreDictionary(JSONDataStore store)
            {
                var storeDictionary = new StoreDictionary(store);
                storeDictionary.LoadDictionary();
                return storeDictionary;
            }

            #region Implementation of IEnumerable

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of ICollection<KeyValuePair<string,string>>

            public void Add(KeyValuePair<string, string> item)
            {
                _dictionary.Add(item);
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                return _dictionary.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                _dictionary.CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                return _dictionary.Remove(item);
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return _dictionary.IsReadOnly; }
            }

            #endregion

            #region Implementation of IDictionary<string,string>

            public bool ContainsKey(string key)
            {
                return _dictionary.ContainsKey(key);
            }

            public void Add(string key, string value)
            {
                _dictionary.Add(key, value);
            }

            public bool Remove(string key)
            {
                return _dictionary.Remove(key);
            }

            public bool TryGetValue(string key, out string value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public string this[string key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public ICollection<string> Keys
            {
                get { return _dictionary.Keys; }
            }

            public ICollection<string> Values
            {
                get { return _dictionary.Values; }
            }

            #endregion

            public void Save()
            {
                var json = JsonConvert.SerializeObject(_dictionary);
                _store._setAccessor(json);
            }
        }

        public Task StoreAsync<T>(string key, T value)
        {
            var store = StoreDictionary.GetStoreDictionary(this);
            store[key] = JsonConvert.SerializeObject(value);
            store.Save();
            return Task.FromResult(0);
        }

        public Task DeleteAsync<T>(string key)
        {
            var store = StoreDictionary.GetStoreDictionary(this);
            store.Remove(key);
            store.Save();
            return Task.FromResult(0);
        }

        public Task<T> GetAsync<T>(string key)
        {
            var store = StoreDictionary.GetStoreDictionary(this);
            string value;
            return Task.FromResult(
                store.TryGetValue(key, out value)
                    ? JsonConvert.DeserializeObject<T>(value)
                    : default(T));
        }

        public Task ClearAsync()
        {
            var store = StoreDictionary.GetStoreDictionary(this);
            store.Clear();
            store.Save();
            return Task.FromResult(0);
        }
    }
}