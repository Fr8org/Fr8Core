using System;
using Newtonsoft.Json.Linq;

namespace Data.Crates
{
    public class Crate<T> : Crate
    {
        public T Value
        {
            get { return Get<T>(); }
        }

        public Crate(Crate crate) 
            : base(crate.ManifestType, crate.Id)
        {
            Label = crate.Label;
            Content = crate.Get<T>();
        }

        public static Crate<T> FromContent(string label, T content)
        {
            return new Crate<T>(Crate.FromContent(label, content));
        }
    }

    public class Crate
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly CrateManifestType _manifestType;
        protected object Content;
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
            get { return Content != null; }
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
                Content = content
            };
        }

        /**********************************************************************************/

        public static Crate FromContent(string label, object content)
        {
            return new Crate(GetManifest(content))
            {
                Label =  label,
                Content = content
            };
        }

        /**********************************************************************************/

        public static Crate FromContent(object content, string id)
        {
            return new Crate(GetManifest(content), id)
            {
                Content = content
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

        public static Crate FromJson(CrateManifestType manifestType, string id, JToken content)
        {
            return new Crate(manifestType, id)
            {
                _rawContent = content
            };
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

        public void Put(object content)
        {
            if (GetManifest(content) != ManifestType)
            {
                throw new ArgumentException("Content manifest is not compatible with crate manifest", "content");
            }

            Content = content;
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
            return (T)Content;
        }

        /**********************************************************************************/

        public override string ToString()
        {
            return string.Format("{1} [{0}]", Id, Label);
        }

        /**********************************************************************************/
    }
}