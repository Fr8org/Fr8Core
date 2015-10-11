using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.MultiTenantObjects;
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

        public Data.Entities.MT_Object GetOrCreateMT_Object(IUnitOfWork _uow, BaseMultiTenantObject curMTO, Type curDataType, Dictionary<Type, Data.Entities.MT_FieldType> typesDict)
        {
            var correspondingMTObject = _uow.MTObjectRepository.FindOne(a => a.MT_OrganizationId == curMTO.fr8AccountId && a.Name == curMTO.Name);
            if (correspondingMTObject == null)
            {
                correspondingMTObject = new Data.Entities.MT_Object() { MT_OrganizationId = curMTO.fr8AccountId, Name = curMTO.Name };
                var correspongdintDTOrganization = _uow.MTOrganizationRepository.GetByKey(curMTO.fr8AccountId);
                correspondingMTObject.MT_Organization = correspongdintDTOrganization;
                correspondingMTObject.MT_FieldType = _mtFieldType.GetOrCreateMT_FieldType(_uow, curDataType, typesDict);
                _uow.MTObjectRepository.Add(correspondingMTObject);
            }
            return correspondingMTObject;
        }
    }
}
