using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.MultiTenantObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            //get or create DTObject
            var correspondingDTObject = _uow.MTObjectRepository.FindOne(a => a.MT_OrganizationId == curData.fr8AccountId && a.Name == curData.Name);
            if (correspondingDTObject == null)
            {
                correspondingDTObject = new MT_Object() { MT_OrganizationId = curData.fr8AccountId, Name = curData.Name };
                _uow.MTObjectRepository.Add(correspondingDTObject);
            }

            //get or create DTFields
            var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id);
            var curDataProperties = curData.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            if (correspondingDTFields.Count() == 0)
            {
                //here we can filter what properties should be added and stored
                for (int i = 1; i < curDataProperties.Count + 1; i++)
                {
                    var property = curDataProperties[i - 1];
                    MT_Field mtField = new MT_Field();
                    mtField.FieldColumnOffset = i;
                    mtField.MT_ObjectId = correspondingDTObject.Id;
                    mtField.Name = property.Name;

                    //get or create FieldType
                    var correspondingMTFieldType = _uow.MTFieldTypeRepository.GetQuery().Where(a => a.TypeName == property.PropertyType.FullName).FirstOrDefault();
                    if (correspondingMTFieldType == null)
                    {
                        correspondingMTFieldType = new MT_FieldType();
                        correspondingMTFieldType.AssemblyName = property.PropertyType.AssemblyQualifiedName;
                        correspondingMTFieldType.TypeName = property.PropertyType.FullName;
                        _uow.MTFieldTypeRepository.Add(correspondingMTFieldType);
                    }
                    mtField.MT_FieldType = correspondingMTFieldType;

                    _uow.MTFieldRepository.Add(mtField);
                }
            }

            //create DTData and fill values
            var data = new MT_Data();
            data.CreatedAt = DateTime.Now;
            data.UpdatedAt = DateTime.Now;
            Map(curData, curDataProperties, data, correspondingDTFields, correspondingDTObject);
            _uow.MTDataRepository.Add(data);
        }



        public void Update(BaseMultiTenantObject curData)
        {
            var correspondingDTObject = _uow.MTObjectRepository.FindOne(a => a.MT_OrganizationId == curData.fr8AccountId && a.Name == curData.Name);
            var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id);
            var curDataProperties = curData.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var data = new MT_Data();
            data.UpdatedAt = DateTime.Now;
            Map(curData, curDataProperties, data, correspondingDTFields, correspondingDTObject);
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
            if (correspondingMT_Data == null) return null;

            var correspondingMT_Object = _uow.MTObjectRepository.FindOne(a => a.Id == correspondingMT_Data.MT_ObjectId && a.Name == correspondingMT_Data.Name);
            if (correspondingMT_Object == null) return null;

            var correspondingMT_Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == correspondingMT_Object.Id);


            return new BaseMultiTenantObject();
        }


        //maps BaseMTO to MTData
        private void Map(BaseMultiTenantObject curData, List<PropertyInfo> curDataProperties, MT_Data data, IEnumerable<MT_Field> correspondingDTFields, MT_Object correspondingDTObject)
        {
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
        private BaseMultiTenantObject MapBack(MT_Data data, IEnumerable<MT_Field> correspondingDTFields, MT_Object correspondingDTObject)
        {
            var objMTType = correspondingDTObject.MT_FieldType;

            BaseMultiTenantObject obj = Activator.CreateInstanceFrom(objMTType.AssemblyName, objMTType.TypeName).Unwrap() as BaseMultiTenantObject;
            obj.fr8AccountId = data.fr8AccountId;
            obj.Name = correspondingDTObject.Name;

            var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var dataValueCells = data.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();

            foreach (var DTField in correspondingDTFields)
            {
                var correspondingProperty = properties.Where(a => a.Name == DTField.Name).FirstOrDefault();
                var valueCell = dataValueCells.Where(a => a.Name == "Value" + DTField.FieldColumnOffset).FirstOrDefault();
                var val = valueCell.GetValue(data);
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