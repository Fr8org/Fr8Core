using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Hub.Enums;

namespace Hub.Interfaces
{
    public interface IField
    {
        bool Exists(FieldValidationDTO data);
    }
}