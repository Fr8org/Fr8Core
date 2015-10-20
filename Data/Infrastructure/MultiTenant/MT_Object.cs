using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.ManifestSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Infrastructure.MultiTenant
{
    public class MT_Object : IMT_Object
    {
        private MT_Field _mtField;
        private MT_FieldType _mtFieldType;

        public MT_Object()
        {
            this._mtField = new MT_Field();
            this._mtFieldType = new MT_FieldType();
        }

        public Data.Entities.MT_Object GetOrCreateMT_Object(IUnitOfWork _uow, Manifest curManifest, Type curDataType)
        {
            var corFieldType = _mtFieldType.GetOrCreateMT_FieldType(_uow, curDataType);

            var correspondingMTObject = _uow.MTObjectRepository.FindOne(a => a.ManifestId == curManifest.ManifestId && a.MT_FieldType == corFieldType);
            if (correspondingMTObject == null)
            {
                correspondingMTObject = new Data.Entities.MT_Object() { Name = curManifest.ManifestName, MT_FieldType = corFieldType, ManifestId = curManifest.ManifestId };
                _uow.MTObjectRepository.Add(correspondingMTObject);
            }
            return correspondingMTObject;
        }
    }
}
