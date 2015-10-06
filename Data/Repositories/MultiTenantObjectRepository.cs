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
    public class MultiTenantObjectRepository : IDisposable
    {
        private IUnitOfWork _uow;
        public MultiTenantObjectRepository(IUnitOfWork uow)
        {
            this._uow = uow;
        }

        public void Add(BaseMultiTenantObject curData)
        {
            var curDataType = curData.GetType();
            var curDataProperties = curDataType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var typesDict = new Dictionary<Type, MT_FieldType>();
            //get or create MTObject
            var correspondingMTObject = _uow.MTObjectRepository.FindOne(a => a.MT_OrganizationId == curData.fr8AccountId && a.Name == curData.Name);
            if (correspondingMTObject == null)
            {
                correspondingMTObject = new MT_Object() { MT_OrganizationId = curData.fr8AccountId, Name = curData.Name };
                var correspongdintDTOrganization = _uow.MTOrganizationRepository.GetByKey(curData.fr8AccountId);
                correspondingMTObject.MT_Organization = correspongdintDTOrganization;
                correspondingMTObject.MT_FieldType = GetOrCreateMTFieldType(curDataType, typesDict);
                _uow.MTObjectRepository.Add(correspondingMTObject);
            }

            //get or create MTFields
            if (correspondingMTObject.Fields == null)
            {
                var fieldsList = new List<MT_Field>();
                //here we can filter what properties should be added and stored
                for (int i = 1; i < curDataProperties.Count + 1; i++)
                {
                    var property = curDataProperties[i - 1];
                    MT_Field mtField = new MT_Field();
                    mtField.FieldColumnOffset = i;
                    mtField.MT_ObjectId = correspondingMTObject.Id;
                    mtField.Name = property.Name;
                    mtField.MT_Object = correspondingMTObject;
                    //get or create FieldType
                    mtField.MT_FieldType = GetOrCreateMTFieldType(property.PropertyType, typesDict);
                    fieldsList.Add(mtField);
                    _uow.MTFieldRepository.Add(mtField);
                }
                //have to requery
                correspondingMTObject.Fields = fieldsList;
            }

            //create MTData and fill values
            var data = new MT_Data();
            data.Name = curData.Name;
            data.CreatedAt = DateTime.Now;
            data.UpdatedAt = DateTime.Now;
            data.MT_Object = correspondingMTObject;
            Map(curData, curDataProperties, data, correspondingMTObject);
            _uow.MTDataRepository.Add(data);
        }

        private MT_FieldType GetOrCreateMTFieldType(Type type, Dictionary<Type, MT_FieldType> newTypes)
        {
            var correspondingMTFieldType = _uow.MTFieldTypeRepository.GetAll().Where(a => a.TypeName == type.FullName).FirstOrDefault();
            // I haven't found a way to get an MTFieldType item from repository, if it wasn't saved yet, so I have a local Dictionary
            if (correspondingMTFieldType == null)
            {
                if (newTypes.ContainsKey(type))
                    correspondingMTFieldType = newTypes[type];
            }
            if (correspondingMTFieldType == null)
            {
                correspondingMTFieldType = new MT_FieldType();
                correspondingMTFieldType.AssemblyName = type.Assembly.FullName;
                correspondingMTFieldType.TypeName = type.FullName;
                _uow.MTFieldTypeRepository.Add(correspondingMTFieldType);
                newTypes[type] = correspondingMTFieldType;
            }
            return correspondingMTFieldType;
        }


        public void Update(BaseMultiTenantObject curData)
        {
            var correspondingDTObject = _uow.MTObjectRepository.FindOne(a => a.MT_OrganizationId == curData.fr8AccountId && a.Name == curData.Name);
            var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id);
            var curDataProperties = curData.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var data = _uow.MTDataRepository.GetByKey(curData.MT_DataId);
            data.UpdatedAt = DateTime.Now;
            Map(curData, curDataProperties, data, correspondingDTObject);
        }

        public void Remove(int id)
        {
            var dataObject = _uow.MTDataRepository.GetByKey(id);
            if (dataObject != null)
                dataObject.IsDeleted = true;
        }

        public BaseMultiTenantObject GetByKey(int id)
        {
            var correspondingMT_Data = _uow.MTDataRepository.GetByKey(id);
            if (correspondingMT_Data == null || correspondingMT_Data.IsDeleted) return null;

            var correspondingMT_Object = _uow.MTObjectRepository.FindOne(a => a.Id == correspondingMT_Data.MT_ObjectId && a.Name == correspondingMT_Data.Name);
            if (correspondingMT_Object == null) return null;

            var correspondingMT_Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == correspondingMT_Object.Id);

            return MapBack(correspondingMT_Data, correspondingMT_Object);
        }


        //maps BaseMTO to MTData
        private void Map(BaseMultiTenantObject curData, List<PropertyInfo> curDataProperties, MT_Data data, MT_Object correspondingDTObject)
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
        private BaseMultiTenantObject MapBack(MT_Data data, MT_Object correspondingDTObject)
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


        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}