using System;
using System.Collections.Generic;
using System.Linq;
using Fr8Data.Constants;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Manifests
{
    public class StandardPayloadDataCM : Manifest
    {
        [ManifestField(IsHidden = true)]
        public string Name { get; set; }

        public List<PayloadObjectDTO> PayloadObjects { get; set; }

        [ManifestField(IsHidden = true)]
        public string ObjectType { get; set; }
        
        
        public StandardPayloadDataCM()
			  :base(MT.StandardPayloadData)
        {
            PayloadObjects = new List<PayloadObjectDTO>();
            ObjectType = "Unspecified";
        }

        public StandardPayloadDataCM(IEnumerable<FieldDTO> fields)
            : this()
        {
            PayloadObjects = new List<PayloadObjectDTO>();
            PayloadObjects.Add(new PayloadObjectDTO(fields));
        }

        public StandardPayloadDataCM(params FieldDTO[] fields)
            :this((IEnumerable<FieldDTO>)fields)
        {
        }

        public List<FieldDTO> GetAllFields()
        {
            var fields = new List<FieldDTO>();

            if (PayloadObjects != null)
            {
                fields.AddRange(PayloadObjects.Where(x=>x.PayloadObject != null).SelectMany(x => x.PayloadObject));
            }

            return fields;
        }

        public bool TryGetValue(string key, bool skipNull, bool ignoreCase, out string value)
        {
            if (PayloadObjects == null)
            {
                value = null;
                return false;
            }

            foreach (var payloadObjectDto in PayloadObjects)
            {
                if (payloadObjectDto.TryGetValue(key, skipNull, ignoreCase, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }


        public string GetValueOrDefault(string key, bool skipNull = false, bool ignoreCase = false)
        {
            string value;

            TryGetValue(key, skipNull, ignoreCase, out value);

            return value;
        }

        public IEnumerable<string> GetValues(string key, bool ignoreCase = false)
        {
            if (PayloadObjects == null)
            {
                yield break;
            }

            foreach (var payloadObjectDto in PayloadObjects)
            {
                foreach (var value in payloadObjectDto.GetValues(key, ignoreCase))
                {
                    yield return value;
                }
            }
        }
        
        public bool HasValues()
        {
            if (PayloadObjects == null)
            {
                return false;
            }

            foreach (var payloadObjectDto in PayloadObjects)
            {
                if (payloadObjectDto.PayloadObject != null)
                {
                    if (payloadObjectDto.PayloadObject.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerable<FieldDTO> AllValues()
        {
            if (PayloadObjects == null)
            {
                yield break;
            }

            foreach (var payloadObjectDto in PayloadObjects)
            {
                if (payloadObjectDto.PayloadObject != null)
                {
                    foreach (var value in payloadObjectDto.PayloadObject)
                    {
                        yield return value;
                    }
                }
            }
        }
    }


    public class PayloadObjectDTO
    {
        public List<FieldDTO> PayloadObject;


        public PayloadObjectDTO()
        {
            PayloadObject = new List<FieldDTO>();
        }

        public PayloadObjectDTO(IEnumerable<FieldDTO> fieldData)
        {
            PayloadObject = new List<FieldDTO>(fieldData);
        }

        public PayloadObjectDTO(params FieldDTO[] fieldData) : this(fieldData as IEnumerable<FieldDTO>)
        {
        }

        public bool TryGetValue(string key, bool skipNull, bool ignoreCase, out string value)
        {
            if (PayloadObject == null)
            {
                value = null;
                return false;
            }

            var stringComparison = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            foreach (var fieldDto in PayloadObject)
            {
                if (string.Equals(fieldDto.Key, key, stringComparison))
                {
                    if (skipNull && fieldDto.Value == null)
                    {
                        continue;
                    }

                    value = fieldDto.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public string GetValue(string key, bool ignoreCase = false)
        {
            var stringComparison = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            var pair = PayloadObject.FirstOrDefault(x => string.Equals(x.Key, key, stringComparison));
            
            if (pair == null)
            {
                throw new KeyNotFoundException(string.Format("Unable to find the key {0}", key));
            }

            return pair.Value;
        }

        public IEnumerable<string> GetValues(string key, bool ignoreCase = false)
        {
            if (PayloadObject == null)
            {
               yield break;
            }

            var stringComparison = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            foreach (var fieldDto in PayloadObject)
            {
                if (string.Equals(fieldDto.Key, key, stringComparison))
                {
                    yield return fieldDto.Value;
                }
            }
        }
    }


}
