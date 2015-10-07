using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.MultiTenantObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class MultiTenantObjectRepository
    {
        public void Add(IUnitOfWork _uow, BaseMultiTenantObject curMTO)
        {
            var curDataType = curMTO.GetType();
            var curDataProperties = curDataType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var typesDict = new Dictionary<Type, MT_FieldType>();
            //get or create MTObject
            var fieldWorker = new Infrastructure.MultiTenant.MT_Field();
            var correspondingMTObject = fieldWorker.GetOrCreateMT_Object(_uow, curMTO, curDataType, typesDict);

            if (correspondingMTObject.Fields == null)
            {
                correspondingMTObject.Fields = GetOrCreateMT_Fields(_uow, curDataProperties, correspondingMTObject, typesDict);
            }

            //create MTData and fill values
            var data = new MT_Data();
            data.Name = curMTO.Name;
            data.CreatedAt = DateTime.Now;
            data.UpdatedAt = DateTime.Now;
            data.MT_Object = correspondingMTObject;
            MapObjectToMTData(curMTO, curDataProperties, data, correspondingMTObject);
            _uow.MTDataRepository.Add(data);
        }

        private List<MT_Field> GetOrCreateMT_Fields(IUnitOfWork _uow, List<PropertyInfo> curDataProperties, MT_Object correspondingMTObject, Dictionary<Type, MT_FieldType> typesDict)
        {
            var fieldsList = new List<MT_Field>();
            int i = 1;
            foreach (var property in curDataProperties)
            {
                MT_Field mtField = new MT_Field();
                mtField.FieldColumnOffset = i;
                mtField.MT_ObjectId = correspondingMTObject.Id;
                mtField.Name = property.Name;
                mtField.MT_Object = correspondingMTObject;
                //get or create FieldType
                var fieldWorker = new Infrastructure.MultiTenant.MT_Field();
                mtField.MT_FieldType = fieldWorker.GetOrCreateMT_FieldType(_uow, property.PropertyType, typesDict);
                fieldsList.Add(mtField);
                _uow.MTFieldRepository.Add(mtField);
                i++;
            }
            return fieldsList;
        }

    

        public void Update(IUnitOfWork _uow, BaseMultiTenantObject curData)
        {
            var correspondingDTObject = _uow.MTObjectRepository.FindOne(a => a.MT_OrganizationId == curData.fr8AccountId && a.Name == curData.Name);
            var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id);
            var curDataProperties = curData.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var data = _uow.MTDataRepository.GetByKey(curData.MT_DataId);
            data.UpdatedAt = DateTime.Now;
            MapObjectToMTData(curData, curDataProperties, data, correspondingDTObject);
        }

        public void Remove(IUnitOfWork _uow, int id)
        {
            var dataObject = _uow.MTDataRepository.GetByKey(id);
            if (dataObject != null)
                dataObject.IsDeleted = true;
        }

        public BaseMultiTenantObject GetByKey(IUnitOfWork _uow, int id)
        {
            var correspondingMT_Data = _uow.MTDataRepository.GetByKey(id);
            if (correspondingMT_Data == null || correspondingMT_Data.IsDeleted) return null;

            var correspondingMT_Object = _uow.MTObjectRepository.FindOne(a => a.Id == correspondingMT_Data.MT_ObjectId && a.Name == correspondingMT_Data.Name);
            if (correspondingMT_Object == null) return null;

            var correspondingMT_Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == correspondingMT_Object.Id);

            return MapMTDataToObject(correspondingMT_Data, correspondingMT_Object);
        }

        //maps BaseMTO to MTData
        private void MapObjectToMTData(BaseMultiTenantObject curData, List<PropertyInfo> curDataProperties, MT_Data data, MT_Object correspondingDTObject)
        {
            var correspondingDTFields = correspondingDTObject.Fields;
            curData.MT_DataId = data.Id;
            data.fr8AccountId = curData.fr8AccountId;
            data.GUID = Guid.Empty;
            data.MT_ObjectId = correspondingDTObject.Id;
            var dataValueCells = data.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            foreach (var field in correspondingDTFields)
            {
                var property = curDataProperties.Where(a => a.Name == field.Name).FirstOrDefault();
                var corrDataCell = dataValueCells.Where(a => a.Name == "Value" + field.FieldColumnOffset).FirstOrDefault();
                var val = property.GetValue(curData);
                corrDataCell.SetValue(data, val);
            }
        }

        //instantiate object from MTData
        private BaseMultiTenantObject MapMTDataToObject(MT_Data data, MT_Object correspondingDTObject)
        {

            var correspondingDTFields = correspondingDTObject.Fields;
            var objMTType = correspondingDTObject.MT_FieldType;
            BaseMultiTenantObject obj = Activator.CreateInstance(objMTType.AssemblyName, objMTType.TypeName).Unwrap() as BaseMultiTenantObject;
            obj.fr8AccountId = data.fr8AccountId;
            obj.Name = correspondingDTObject.Name;
            obj.MT_DataId = data.Id;
            var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var dataValueCells = data.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();

            foreach (var DTField in correspondingDTFields)
            {
                var correspondingProperty = properties.Where(a => a.Name == DTField.Name).FirstOrDefault();
                var valueCell = dataValueCells.Where(a => a.Name == "Value" + DTField.FieldColumnOffset).FirstOrDefault();

                object val = null;
                if (!correspondingProperty.PropertyType.IsValueType)
                    val = valueCell.GetValue(data);
                else
                {
                    object boxedObject = RuntimeHelpers.GetObjectValue(correspondingProperty);
                }

                correspondingProperty.SetValue(obj, val);
            }
            return obj;
        }
    }
}