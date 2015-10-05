using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;
using JsonSerializer = Utilities.Serializers.Json.JsonSerializer;

namespace Core.Services
{
    public class Crate : ICrate
    {
        public Crate()
        {
        }

        public CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0)
        {
            var crateDTO = new CrateDTO() 
            { 
                Id = Guid.NewGuid().ToString(), 
                Label = label, 
                Contents = contents, 
                ManifestType = manifestType, 
                ManifestId = manifestId 
            };
            return crateDTO;
        }

        public CrateDTO CreateAuthenticationCrate(string label, AuthenticationMode mode, string url = null)
        {
            var manifestSchema = new StandardAuthenticationMS()
            {
                Mode = mode,
                Url = url
            };

            return Create(
                label,
                JsonConvert.SerializeObject(manifestSchema),
                manifestType: CrateManifests.STANDARD_AUTHENTICATION_NAME,
                manifestId: CrateManifests.STANDARD_AUTHENTICATION_ID);
        }

        public CrateDTO CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields)
        {    
            return Create(label, 
                JsonConvert.SerializeObject(new StandardDesignTimeFieldsMS() {Fields = fields.ToList()}),
                manifestType: CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, 
                manifestId: CrateManifests.DESIGNTIME_FIELDS_MANIFEST_ID);
        }

        public CrateDTO CreateStandardConfigurationControlsCrate(string label, params ControlsDefinitionDTO[] controls)
        {
            return Create(label, 
                JsonConvert.SerializeObject(new StandardConfigurationControlsMS() { Controls = controls.ToList() }),
                manifestType: CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                manifestId: CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_ID);
        }

        public CrateDTO CreateStandardEventSubscriptionsCrate(string label, params string[] subscriptions)
        {
            return Create(label,
                JsonConvert.SerializeObject(new EventSubscriptionMS() { Subscriptions = subscriptions.ToList() }),
                manifestType: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME,
                manifestId: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_ID);
        }

        public CrateDTO CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table)
        {
            return Create(label,
                JsonConvert.SerializeObject(new StandardTableDataMS() { Table = table.ToList(), FirstRowHeaders = firstRowHeaders }),
                manifestType: CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME,
                manifestId: CrateManifests.STANDARD_TABLE_DATA_MANIFEST_ID);
        }

        public T GetContents<T>(CrateDTO crate)
        {
            return JsonConvert.DeserializeObject<T>(crate.Contents);
        }

        /// <summary>
        /// Retrieves all JObject elements that have a key field equal to a key value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="searchCrates">Crates collection where to search through. CrateDTO.Contents property is used.</param>
        /// <param name="key">Key field value to search</param>
        /// <param name="keyFieldName">Key field name</param>
        /// <returns>Returns JSON descendants with specified key field value.</returns>
        /// <remarks>This method iterates through all JSON descendants (entire JSON tree).</remarks>
        /// <example>
        /// var crates = new[] { new CrateDTO { Contents: "[{key: 'example1', value: 'some value'}, {key: 'example2', value: 'another value'}, {name: 'example1', value: 'note there is no key field'}]" } };
        /// var elements = GetElementByKey(crates, "example1", "key");
        /// // elements will contain the only JObject: {key: 'example1', value: 'some value'}
        /// </example>
        public IEnumerable<JObject> GetElementByKey<TKey>(IEnumerable<CrateDTO> searchCrates, TKey key, string keyFieldName)
        {
            List<JObject> resultsObjects = new List<JObject>();
            foreach (var curCrate in searchCrates.Where(c => !string.IsNullOrEmpty(c.Contents)))
            {
                JContainer curCrateJSON = JsonConvert.DeserializeObject<JContainer>(curCrate.Contents);
                var results = curCrateJSON.Descendants()
                    .OfType<JObject>()
                    // where (object has a key field) && (key field value equals to key argument)
                    .Where(x => x[keyFieldName] != null && x[keyFieldName].Value<TKey>().Equals(key));
                resultsObjects.AddRange(results); ;
            }
            return resultsObjects;
        }

        public void RemoveCrateByManifestId(IList<CrateDTO> crates, int manifestId)
        {
            var curCrates = crates.Where(c => c.ManifestId == manifestId).ToList();
            if (curCrates.Count() > 0)
            {
                foreach (CrateDTO crate in curCrates)
                {
                    crates.Remove(crate);
                }
            }
        }

        public void RemoveCrateByLabel(IList<CrateDTO> crates, string label)
        {
            var curCrates = crates.Where(c => c.Label == label).ToList();
            if (curCrates.Count() > 0)
            {
                foreach (CrateDTO crate in curCrates)
                {
                    crates.Remove(crate);
                }
            }
        }

        public void RemoveCrateByManifestType(IList<CrateDTO> crates, string manifestType)
        {
            var curCrates = crates.Where(c => c.ManifestType == manifestType).ToList();
            if (curCrates.Count() > 0)
            {
                foreach (CrateDTO crate in curCrates)
                {
                    crates.Remove(crate);
                }
            }
        }
    }
}
