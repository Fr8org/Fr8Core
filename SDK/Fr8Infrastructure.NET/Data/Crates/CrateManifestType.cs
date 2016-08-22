using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Fr8.Infrastructure.Data.Crates
{
    public struct CrateManifestType
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        public static readonly CrateManifestType Unknown = new CrateManifestType(null, 0);
        public static readonly CrateManifestType Any = new CrateManifestType(null, Int32.MinValue);

        /**********************************************************************************/
        [JsonProperty("type")]
        public readonly string Type;

        [JsonProperty("id")]
        public readonly int Id;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public CrateManifestType(string type, int id)
        {
            Type = type;
            Id = id;
        }


        /**********************************************************************************/

        public static CrateManifestType FromEnum(Enum manifestType)
        {
            var enumType = manifestType.GetType();
            var member = enumType.GetMember(manifestType.ToString());

            if (Enum.GetUnderlyingType(enumType) != typeof (int))
            {
                throw new ArgumentException("Only integer enum are supported", "manifestType");
            }

            if (member.Length == 0)
            {
                throw new ArgumentException("Invalid value for manifest type", "manifestType");
            }
            
            var crateManifesetAttr = (CrateManifestTypeDescriptionAttribute) member[0].GetCustomAttribute(typeof (CrateManifestTypeDescriptionAttribute));
            
            if (crateManifesetAttr == null)
            {
                throw new ArgumentException(string.Format("Manifest type {0} doesn't have CrateManifestTypeDescription attribute set.", manifestType), "manifestType");
            }
            
            return new CrateManifestType(crateManifesetAttr.TypeName, Convert.ToInt32(manifestType));
        }

        /**********************************************************************************/

        public override string ToString()
        {
            return string.Format("{0} ({1})", Type, Id);
        }

        /**********************************************************************************/

        public bool Equals(CrateManifestType other)
        {
            if (Id == other.Id)
            {
                return true;
            }

            if (Id == Unknown.Id || other.Id == Unknown.Id)
            {
                return false;
            }

            return Id == Any.Id || other.Id == Any.Id;
        }

        /**********************************************************************************/

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CrateManifestType && Equals((CrateManifestType) obj);
        }

        /**********************************************************************************/

        public override int GetHashCode()
        {
            unchecked
            {
                return /*((Type != null ? Type.GetHashCode() : 0)*397) ^*/ Id;
            }
        }

        /**********************************************************************************/

        public static bool operator ==(CrateManifestType first, CrateManifestType second)
        {
            return first.Equals(second);
        }

        /**********************************************************************************/

        public static bool operator !=(CrateManifestType first, CrateManifestType second)
        {
            return !first.Equals(second);
        }

        /**********************************************************************************/
    }
}