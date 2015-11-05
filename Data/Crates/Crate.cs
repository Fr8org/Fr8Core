using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Crates
{
    [JsonConverter(typeof(DenySerizalitionConverter), "Crate can't be directly serialized to JSON. Convert it to CrateDTO.")]
    public class Crate
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly CrateManifestType _manifestType;
        protected object KnownContent;
        private JToken _rawContent;

        /**********************************************************************************/

        public readonly string Id;
        
        /**********************************************************************************/

        public string Label
        {
            get; 
            set;
        }

        /**********************************************************************************/

        public bool IsKnownManifest
        {
            get { return KnownContent != null; }
        }

        /**********************************************************************************/

        public CrateManifestType ManifestType
        {
            get { return _manifestType; }
        }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public Crate(CrateManifestType manifestType, string id)
        {
            _manifestType = manifestType;
            Id = id;
        }
        
        /**********************************************************************************/

        public Crate(CrateManifestType manifestType)
        {
            _manifestType = manifestType;
            Id = Guid.NewGuid().ToString();
        }

        /**********************************************************************************/

        public static Crate FromContent(object content)
        {
            return new Crate(GetManifest(content))
            {
                KnownContent = content
            };
        }

        /**********************************************************************************/

        public static Crate FromContent(string label, object content)
        {
            return new Crate(GetManifest(content))
            {
                Label =  label,
                KnownContent = content
            };
        }
        
        /**********************************************************************************/

        public static Crate FromJson(string label, JToken content)
        {
            return new Crate(CrateManifestType.Unknown)
            {
                Label = label,
                _rawContent = content
            };
        }

        /**********************************************************************************/

        public static Crate FromJson(CrateManifestType manifestType, JToken content)
        {
            return new Crate(manifestType)
            {
                _rawContent = content
            };
        }

        /**********************************************************************************/

        internal static Crate FromJson(CrateManifestType manifestType, string id, JToken content)
        {
            return new Crate(manifestType, id)
            {
                _rawContent = content
            };
        }

        /**********************************************************************************/

        internal static Crate FromContent(object content, string id)
        {
            return new Crate(GetManifest(content), id)
            {
                KnownContent = content
            };
        }
      

        /**********************************************************************************/

        public void Put(object content)
        {
            if (GetManifest(content) != ManifestType)
            {
                throw new ArgumentException("Content manifest is not compatible with crate manifest", "content");
            }

            KnownContent = content;
            _rawContent = null;
        }

        /**********************************************************************************/

        public JToken GetRaw()
        {
            return _rawContent;
        }

        /**********************************************************************************/

        public T Get<T>() 
        {
            return (T)KnownContent;
        }

        /**********************************************************************************/

        public override string ToString()
        {
            return string.Format("{1} [{0}]", Id, Label);
        }

        /**********************************************************************************/

        private static CrateManifestType GetManifest(object content)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(content, out manifestType))
            {
                throw new ArgumentException("Content is not marked with CrateManifestAttribute", "content");
            }

            return manifestType;
        }

        /**********************************************************************************/
    }
}