using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class Field
    {
        /// <summary>
        /// Creates list of fields for a corresponding MTObject.
        /// </summary>
        /// <param name="_uow"></param>
        /// <param name="curDataProperties"></param>
        /// <param name="correspondingMTObject"></param>
        /// <param name="typesDict"></param>
        /// <returns></returns>
        private List<MT_Field> CreateMT_Fields(IUnitOfWork _uow, List<PropertyInfo> curDataProperties, MT_Object correspondingMTObject, Dictionary<Type, MT_FieldType> typesDict)
        {
            var fieldsList = new List<MT_Field>();
            //here we can filter what properties should be added and stored

            int i = 1;
            foreach (var property in curDataProperties)
            {
                MT_Field mtField = new MT_Field();
                mtField.FieldColumnOffset = i;
                mtField.MT_ObjectId = correspondingMTObject.Id;
                mtField.Name = property.Name;
                mtField.MT_Object = correspondingMTObject;
                //get or create FieldType
                mtField.MT_FieldType = GetOrCreateMT_FieldType(_uow, property.PropertyType, typesDict);
                fieldsList.Add(mtField);
                _uow.MTFieldRepository.Add(mtField);
                i++;
            }
            return fieldsList;
        }


        /// <summary>
        /// Gets or creates MT_FieldType. Unsaved MT_FieldType that already have been added should be passed as a "Dictionary<Type, MT_FieldType>  newFieldTypesInContext"
        /// </summary>
        /// <param name="_uow"></param>
        /// <param name="type"></param>
        /// <param name="newFieldTypesInContext">unsaved types</param>
        /// <returns></returns>
        public MT_FieldType GetOrCreateMT_FieldType(IUnitOfWork _uow, Type type, Dictionary<Type, MT_FieldType> newFieldTypesInContext)
        {
            var correspondingMTFieldType = _uow.MTFieldTypeRepository.GetAll().Where(a => a.TypeName == type.FullName).FirstOrDefault();
            if (correspondingMTFieldType == null)
            {
                if (newFieldTypesInContext.ContainsKey(type))
                    correspondingMTFieldType = newFieldTypesInContext[type];
            }
            if (correspondingMTFieldType == null)
            {
                correspondingMTFieldType = new MT_FieldType();
                correspondingMTFieldType.AssemblyName = type.Assembly.FullName;
                correspondingMTFieldType.TypeName = type.FullName;
                _uow.MTFieldTypeRepository.Add(correspondingMTFieldType);
                newFieldTypesInContext[type] = correspondingMTFieldType;
            }
            return correspondingMTFieldType;
        }
    }
}
