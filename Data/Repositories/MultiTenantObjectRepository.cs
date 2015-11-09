using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.Manifests;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class MultiTenantObjectRepository
    {
        private Infrastructure.MultiTenant.MT_Field _mtField;
        private Infrastructure.MultiTenant.MT_Data _mtData;
        private Infrastructure.MultiTenant.MT_Object _mtObject;
        private Infrastructure.MultiTenant.MT_FieldType _mtFieldType;

        public MultiTenantObjectRepository()
        {
            this._mtField = new Infrastructure.MultiTenant.MT_Field();
            this._mtData = new Infrastructure.MultiTenant.MT_Data();
            this._mtObject = new Infrastructure.MultiTenant.MT_Object();
            this._mtFieldType = new Infrastructure.MultiTenant.MT_FieldType();
        }

        public void Add(IUnitOfWork _uow, Manifest curManifest, string curFr8AccountId)
        {
            var curDataType = curManifest.GetType();
            var curDataProperties = curDataType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            //get or create MTObject
            var correspondingMTObject = _mtObject.GetOrCreateMT_Object(_uow, curManifest, curDataType);
            if (correspondingMTObject.Fields == null)
            {
                correspondingMTObject.Fields = _mtField.CreateList(_uow, curDataProperties, correspondingMTObject);
            }

            //create MTData, fill values, and add to repo
            var data = _mtData.Create(curFr8AccountId, curManifest, correspondingMTObject);
            MapManifestToMTData(curFr8AccountId, curManifest, curDataProperties, data, correspondingMTObject);
            _uow.MTDataRepository.Add(data);
            _uow.SaveChanges();
        }

        public void AddOrUpdate<T>(IUnitOfWork _uow, string curFr8AccountId, T curManifest, Expression<Func<T, object>> keyProperty)
            where T : Manifest
        {
            var curManifestType = typeof(T);
            var coorespondingMTFieldType = _mtFieldType.GetOrCreateMT_FieldType(_uow, curManifestType);
            _uow.SaveChanges();
            var currentMTObject =
                _uow.MTObjectRepository.GetQuery().FirstOrDefault(a => a.ManifestId == curManifest.ManifestType.Id &&
                                                                       a.MT_FieldType != null &&
                                                                       a.MT_FieldType.Id == coorespondingMTFieldType.Id);
            if (currentMTObject != null)
            {
                var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == currentMTObject.Id);
                var keyPropertyInfo = GetPropertyInfo(keyProperty);
                MT_Data correspondingMTData = FindMT_DataByKeyField(_uow, curFr8AccountId, curManifest, currentMTObject,
                    correspondingDTFields, keyPropertyInfo);
                if (correspondingMTData != null)
                {
					correspondingMTData.UpdatedAt = DateTime.UtcNow;
                    var curDataProperties =
                        curManifestType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly |
                                                      System.Reflection.BindingFlags.Instance |
                                                      System.Reflection.BindingFlags.Public).ToList();
                    MapManifestToMTData(curFr8AccountId, curManifest, curDataProperties, correspondingMTData, currentMTObject);
                    return;
                }
            }

            //MTObject or MTData is missing
            Add(_uow, curManifest, curFr8AccountId);
        }

        public void Update<T>(IUnitOfWork _uow, string curFr8AccountId, T curManifest, Expression<Func<T, object>> keyProperty) where T : Manifest
        {
            var keyPropertyInfo = GetPropertyInfo(keyProperty);
            var curManifestType = typeof(T);
            var curDTOObjectFieldType = _mtFieldType.GetOrCreateMT_FieldType(_uow, curManifestType);

            var correspondingDTObject = _uow.MTObjectRepository.FindOne(a => a.ManifestId == curManifest.ManifestType.Id && a.MT_FieldType == curDTOObjectFieldType);
            var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id);

            MT_Data correspondingMTData = FindMT_DataByKeyField(_uow, curFr8AccountId, curManifest, correspondingDTObject, correspondingDTFields, keyPropertyInfo);
            if (correspondingMTData == null) throw new Exception(String.Format("MT_Data wasn't found for {0} with {1} == {2}", curManifest.ManifestType.Type, keyPropertyInfo.Name));

			correspondingMTData.UpdatedAt = DateTime.UtcNow;
            var curDataProperties = curManifestType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            MapManifestToMTData(curFr8AccountId, curManifest, curDataProperties, correspondingMTData, correspondingDTObject);
        }

        public void Remove<T>(IUnitOfWork _uow, string curFr8AccountId, Expression<Func<T, object>> conditionOnKeyProperty, int ManifestId = -1) where T : Manifest
        {
            PropertyInfo leftOperand;
            object rightOperand;
            MethodInfo equalMethod;
            MT_Object curMTObject = FindMT_ObjectByExpression(_uow, conditionOnKeyProperty, ManifestId, out leftOperand, out rightOperand, out equalMethod);
            MT_Data curMTData = FindMT_DataByExpression(_uow, curFr8AccountId, leftOperand, rightOperand, equalMethod, curMTObject);
            if (curMTData != null)
                curMTData.IsDeleted = true;
        }

        public T Get<T>(IUnitOfWork _uow, string curFr8AccountId, Expression<Func<T, object>> conditionOnKeyProperty, int ManifestId = -1) where T : Manifest
        {
            PropertyInfo leftOperand;
            object rightOperand;
            MethodInfo equalMethod;
            MT_Object curMTObject = FindMT_ObjectByExpression(_uow, conditionOnKeyProperty, ManifestId, out leftOperand, out rightOperand, out equalMethod);
            MT_Data curMTData = FindMT_DataByExpression(_uow, curFr8AccountId, leftOperand, rightOperand, equalMethod, curMTObject);
            return (curMTData == null) ? null : MapMTDataToManifest<T>(curMTData, curMTObject);
        }

        private MT_Data FindMT_DataByExpression(IUnitOfWork _uow, string curFr8AccountId, PropertyInfo leftOperand, object rightOperand, MethodInfo equalMethod, MT_Object curMTObject)
        {
            MT_Data curMTData = null;
            if (curMTObject != null)
            {
                var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == curMTObject.Id);
                var keyMTField = correspondingDTFields.Where(a => a.Name == leftOperand.Name).FirstOrDefault();
                var corrMTDataProperty = typeof(MT_Data).GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    .Where(a => a.Name == "Value" + keyMTField.FieldColumnOffset).FirstOrDefault();

                var possibleDatas = _uow.MTDataRepository.FindList(a => a.MT_ObjectId == curMTObject.Id && a.fr8AccountId == curFr8AccountId);

                foreach (var data in possibleDatas)
                {
                    if (data.IsDeleted) continue;
                    var val = corrMTDataProperty.GetValue(data);
                    if ((bool)equalMethod.Invoke(val, new object[] { val, rightOperand }))
                        curMTData = data;
                }
            }
            return curMTData;
        }

        private MT_Data FindMT_DataByKeyField<T>(IUnitOfWork _uow, string curFr8AccountId, T curManifest, MT_Object correspondingDTObject, IEnumerable<MT_Field> correspondingDTFields, PropertyInfo keyPropertyInfo) where T : Manifest
        {
            MT_Data correspondingMTData = null;
            var keyValue = keyPropertyInfo.GetValue(curManifest);
            var possibleDatas = _uow.MTDataRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id && a.fr8AccountId == curFr8AccountId);
            var keyMTField = correspondingDTFields.Where(a => a.Name == keyPropertyInfo.Name).FirstOrDefault();
            correspondingMTData = null;
            var corrMTDataProperty = typeof(MT_Data).GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    .Where(a => a.Name == "Value" + keyMTField.FieldColumnOffset).FirstOrDefault();

            if (corrMTDataProperty == null) return null;

            foreach (var data in possibleDatas)
            {
                var val = corrMTDataProperty.GetValue(data);
                if (val == keyValue)
                {
                    correspondingMTData = data;
                    break;
                }
            }
            return correspondingMTData;
        }

        private MT_Object FindMT_ObjectByExpression<T>(IUnitOfWork _uow, Expression<Func<T, object>> conditionOnKeyProperty, int ManifestId, out PropertyInfo leftOperand, out object rightOperand, out MethodInfo equalMethod) where T : Manifest
        {
            Expression expr = conditionOnKeyProperty;
            leftOperand = null;
            rightOperand = null;
            equalMethod = null;
            expr = new VisitorToEvaluateLocalVariables().Visit(conditionOnKeyProperty);
            try
            {
                var compareExpr = (((expr as LambdaExpression).Body as UnaryExpression).Operand as BinaryExpression);
                leftOperand = (compareExpr.Left as MemberExpression).Member as PropertyInfo;
                rightOperand = (compareExpr.Right as ConstantExpression).Value;
                equalMethod = compareExpr.Method;
            }
            catch { throw new Exception("Incorrect lambda expression"); }

            var curMTObjectTypeField = _mtFieldType.GetOrCreateMT_FieldType(_uow, typeof(T));
            MT_Object curMTObject = null;
            if (ManifestId == -1)
            {
                //We assume that there must be a single MTObject                
                curMTObject = _uow.MTObjectRepository.GetQuery().Where(a => a.MT_FieldType.Id == curMTObjectTypeField.Id).SingleOrDefault();
                if (curMTObject != null)
                    curMTObject.Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == curMTObject.Id).ToList();
            }
            else
            {
                curMTObject = _uow.MTObjectRepository.GetQuery().Where(a => a.MT_FieldType.TypeName == curMTObjectTypeField.TypeName && a.ManifestId == ManifestId).SingleOrDefault();
                if (curMTObject != null)
                    curMTObject.Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == curMTObject.Id).ToList();
            }

            return curMTObject;
        }

        //maps BaseMTO to MTData
        private void MapManifestToMTData(string curFr8AccountId, Manifest curManifest, List<PropertyInfo> curDataProperties, MT_Data data, MT_Object correspondingDTObject)
        {
            var correspondingDTFields = correspondingDTObject.Fields;
            data.fr8AccountId = curFr8AccountId;
            data.GUID = Guid.Empty;
            data.MT_ObjectId = correspondingDTObject.Id;
            var dataValueCells = data.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            foreach (var field in correspondingDTFields)
            {
                var property = curDataProperties.Where(a => a.Name == field.Name).FirstOrDefault();
                var corrDataCell = dataValueCells.Where(a => a.Name == "Value" + field.FieldColumnOffset).FirstOrDefault();
                var val = property.GetValue(curManifest);
                corrDataCell.SetValue(data, val);
            }
        }

        //instantiate object from MTData
        private T MapMTDataToManifest<T>(MT_Data data, MT_Object correspondingDTObject)
        {

            var correspondingDTFields = correspondingDTObject.Fields;
            var objMTType = correspondingDTObject.MT_FieldType;
            object obj = Activator.CreateInstance(objMTType.AssemblyName, objMTType.TypeName).Unwrap();
            var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();
            var dataValueCells = data.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToList();

            if (correspondingDTFields != null)
            {
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
            }
            return (T)obj;
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        private class VisitorToEvaluateLocalVariables : ExpressionVisitor
        {
            protected override Expression VisitMember
                (MemberExpression memberExpression)
            {
                // Recurse down to see if we can simplify...
                var expression = Visit(memberExpression.Expression);

                // If we've ended up with a constant, and it's a property or a field,
                // we can simplify ourselves to a constant
                if (expression is ConstantExpression)
                {
                    object container = ((ConstantExpression)expression).Value;
                    var member = memberExpression.Member;
                    if (member is FieldInfo)
                    {
                        object value = ((FieldInfo)member).GetValue(container);
                        return Expression.Constant(value);
                    }
                    if (member is PropertyInfo)
                    {
                        object value = ((PropertyInfo)member).GetValue(container, null);
                        return Expression.Constant(value);
                    }
                }
                return base.VisitMember(memberExpression);
            }
        }
    }
}