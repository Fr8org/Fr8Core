using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Enums;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json.Linq;

namespace Core.Interfaces
{
    public interface IField
    {
        bool Exists(FieldValidationDTO data);
    }
}