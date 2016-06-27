using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json.Linq;

namespace HubTests.Managers
{
    partial class CrateManagerTests
    {
        private static CrateStorageDTO GetKnownManifestsStorageDto(string key = "value")
        {
            var storage = new CrateStorageDTO
            {
                Crates = new[]
                {
                    TestKnownCrateDto("id1", key + "1"),
                    TestKnownCrateDto("id2", key + "2"),
                }
            };

            return storage;
        }

        private static CrateStorageDTO GetUnknownManifestsStorageDto()
        {
            var storage = new CrateStorageDTO
            {
                Crates = new[]
                {
                    TestUnknownCrateDto("id1", "value1"),
                    TestUnknownCrateDto("id2", "value2"),
                }
            };

            return storage;
        }



        private static CrateDTO TestUnknownCrateDto(string id, string value)
        {
            return new CrateDTO
            {
                Label = id + "_label",
                Id = id,
                ManifestId = 888888,
                ManifestType = "Unknwon manifest",
                Contents = value
            };
        }

        private static CrateDTO TestKnownCrateDto(string id, string value)
        {
            var manifest = TestManifest(value);

            return new CrateDTO
            {
                Label = id + "_label",
                Id = id,
                ManifestId = manifest.ManifestType.Id,
                ManifestType = manifest.ManifestType.Type,
                Contents = JToken.FromObject(manifest)
            };
        }

        private static KeyValueListCM TestManifest(string value = "value")
        {
            return new KeyValueListCM
            {
                Values = new List<KeyValueDTO>
                {
                    new KeyValueDTO("key", value)
                }
            };
        }


        private static StandardTableDataCM GetTestTable()
        {
            string tableJson = @"{
          'Table': [
            {
              'Row': [
                {
                  'Cell': {
                    'key': 'SerbianWord',
                    'value': 'Pouzdan',
                    'tags': null,
                    'availability': null
                  }
                },
                {
                  'Cell': {
                    'key': 'EnglishWord',
                    'value': 'Reliable',
                    'tags': null,
                    'availability': null
                  }
                }
              ]
            },
            {
              'Row': [
                {
                  'Cell': {
                    'key': 'SerbianWord',
                    'value': 'Zabolela mi je glava',
                    'tags': null,
                    'availability': null
                  }
                },
                {
                  'Cell': {
                    'key': 'EnglishWord',
                    'value': 'I ve got headache',
                    'tags': null,
                    'availability': null
                  }
                }
              ]
            }
          ],
          'FirstRowHeaders': false
        }";
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StandardTableDataCM>(tableJson);
        }

        private static StandardTableDataCM GetTestTableWithHeaders()
        {
            string tableJson = @"{
          'Table': [
            {
              'Row': [
                {
                  'Cell': {
                    'key': 'SerbianWord',
                    'value': 'SerbianWord',
                    'tags': null,
                    'availability': null
                  }
                },
                {
                  'Cell': {
                    'key': 'EnglishWord',
                    'value': 'EnglishWord',
                    'tags': null,
                    'availability': null
                  }
                }
              ]
            },
            {
              'Row': [
                {
                  'Cell': {
                    'key': 'Pouzdan',
                    'value': 'Pouzdan',
                    'tags': null,
                    'availability': null
                  }
                },
                {
                  'Cell': {
                    'key': 'Reliable',
                    'value': 'Reliable',
                    'tags': null,
                    'availability': null
                  }
                }
              ]
            },
            {
              'Row': [
                {
                  'Cell': {
                    'key': 'Zabolela mi je glava',
                    'value': 'Zabolela mi je glava',
                    'tags': null,
                    'availability': null
                  }
                },
                {
                  'Cell': {
                    'key': 'I ve got headache',
                    'value': 'I ve got headache',
                    'tags': null,
                    'availability': null
                  }
                }
              ]
            }
          ],
          'FirstRowHeaders': true
        }";
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StandardTableDataCM>(tableJson);
        }

    }
}
