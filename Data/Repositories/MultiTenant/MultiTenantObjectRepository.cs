using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Data.Expressions;
using Data.Infrastructure.MultiTenant;
using Data.Interfaces;
using Data.Interfaces.Manifests;

namespace Data.Repositories
{
    public class MultiTenantObjectRepository : IMultiTenantObjectRepository
    {
        private readonly static HashSet<Type> PrimitiveTypes = new HashSet<Type>()
        {
            typeof(string), typeof(bool), typeof(bool?), typeof(byte), typeof(byte?),
            typeof(char), typeof(char?), typeof(short), typeof(short?), typeof(int),
            typeof(int?), typeof(long), typeof(long?), typeof(float), typeof(float?), 
            typeof(double), typeof(double?), typeof(DateTime), typeof(DateTime?)
        };

        private MT_Field _mtField;
        private MT_Data _mtData;
        private MT_Object _mtObject;
        private MT_FieldType _mtFieldType;

        public MultiTenantObjectRepository()
        {
            this._mtField = new MT_Field();
            this._mtData = new MT_Data();
            this._mtObject = new MT_Object();
            this._mtFieldType = new MT_FieldType();
        }

        public void Add(IUnitOfWork _uow, Manifest curManifest, string curFr8AccountId)
        {
            var curDataType = curManifest.GetType();
            var curDataProperties = curDataType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
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

        public void AddOrUpdate<T>(IUnitOfWork _uow, string curFr8AccountId, T curManifest, Expression<Func<T, object>> keyProperty = null)
            where T : Manifest
        {
            if (keyProperty == null)
            {
                keyProperty = BuildKeyPropertyExpression(curManifest);
            }

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
                Entities.MT_Data correspondingMTData = FindMT_DataByKeyField(_uow, curFr8AccountId, curManifest, currentMTObject,
                    correspondingDTFields, keyPropertyInfo);
                if (correspondingMTData != null)
                {
					correspondingMTData.UpdatedAt = DateTime.UtcNow;
                    var curDataProperties =
                        curManifestType.GetProperties(BindingFlags.DeclaredOnly |
                                                      BindingFlags.Instance |
                                                      BindingFlags.Public).ToList();
                    MapManifestToMTData(curFr8AccountId, curManifest, curDataProperties, correspondingMTData, currentMTObject);
                    return;
                }
            }

            //MTObject or MTData is missing
            Add(_uow, curManifest, curFr8AccountId);
        }

        private Expression<Func<T, object>> BuildKeyPropertyExpression<T>(T curManifest)
            where T : Manifest
        {
            var pk = curManifest.GetPrimaryKey();

            if (pk.Length == 0)
            {
                throw new InvalidOperationException(string.Format("Primary key for manifest {0}", curManifest.GetType()));
            }

            if (pk.Length > 1)
            {
                throw new NotSupportedException(string.Format("Composite primary key is not supported. Manifest type: {0}", curManifest.GetType()));
            }

            // Build expression expected by current MT repository implementation that will direct MT repository to use firt PK property as primary key for all operations
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            
            var member = typeof (T).GetProperty(pk[0]);
            
            if (member == null)
            {
                throw new Exception(string.Format("Failed to find property '{0}' specified as part of primary key. Manifest {1}", pk[0], curManifest.GetType()));
            }

            var accessor = Expression.MakeMemberAccess(param, member);

            return Expression.Lambda<Func<T, object>>(accessor, param);
        }

        public void Update<T>(IUnitOfWork _uow, string curFr8AccountId, T curManifest, Expression<Func<T, object>> keyProperty = null) where T : Manifest
        {
            if (keyProperty == null)
            {
                keyProperty = BuildKeyPropertyExpression(curManifest);
            }

            var keyPropertyInfo = GetPropertyInfo(keyProperty);
            var curManifestType = typeof(T);
            var curDTOObjectFieldType = _mtFieldType.GetOrCreateMT_FieldType(_uow, curManifestType);

            var correspondingDTObject = _uow.MTObjectRepository.FindOne(a => a.ManifestId == curManifest.ManifestType.Id && a.MT_FieldType == curDTOObjectFieldType);
            var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id);

            Entities.MT_Data correspondingMTData = FindMT_DataByKeyField(_uow, curFr8AccountId, curManifest, correspondingDTObject, correspondingDTFields, keyPropertyInfo);
            if (correspondingMTData == null) throw new Exception(String.Format("MT_Data wasn't found for {0} with {1} == {2}", curManifest.ManifestType.Type, keyPropertyInfo.Name));

			correspondingMTData.UpdatedAt = DateTime.UtcNow;
            var curDataProperties = curManifestType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
            MapManifestToMTData(curFr8AccountId, curManifest, curDataProperties, correspondingMTData, correspondingDTObject);
        }

        public void Remove<T>(IUnitOfWork _uow, string curFr8AccountId, Expression<Func<T, object>> conditionOnKeyProperty, int ManifestId = -1) where T : Manifest
        {
            PropertyInfo leftOperand;
            object rightOperand;
            MethodInfo equalMethod;
            Entities.MT_Object curMTObject = FindMT_ObjectByExpression(_uow, conditionOnKeyProperty, ManifestId, out leftOperand, out rightOperand, out equalMethod);
            Entities.MT_Data curMTData = FindMT_DataByExpression(_uow, curFr8AccountId, leftOperand, rightOperand, equalMethod, curMTObject);
            if (curMTData != null)
                curMTData.IsDeleted = true;
        }

        public T Get<T>(IUnitOfWork _uow, string curFr8AccountId, Expression<Func<T, object>> conditionOnKeyProperty, int ManifestId = -1) where T : Manifest
        {
            PropertyInfo leftOperand;
            object rightOperand;
            MethodInfo equalMethod;
            Entities.MT_Object curMTObject = FindMT_ObjectByExpression(_uow, conditionOnKeyProperty, ManifestId, out leftOperand, out rightOperand, out equalMethod);
            Entities.MT_Data curMTData = FindMT_DataByExpression(_uow, curFr8AccountId, leftOperand, rightOperand, equalMethod, curMTObject);
            return (curMTData == null) ? null : MapMTDataToManifest<T>(curMTData, curMTObject);
        }

        public IMtQueryable<T> AsQueryable<T>(IUnitOfWork uow, string curFr8AccountId)
            where T : Manifest
        {
            return new MtQueryAll<T>(new MtQueryExecutor<T>(this, uow, curFr8AccountId));
        }

        public List<T> Query<T>(IUnitOfWork uow, string curFr8AccountId, Expression<Func<T, bool>> query)
            where T : Manifest
        {
            var curMTObjectTypeField = _mtFieldType.GetOrCreateMT_FieldType(uow, typeof(T));
            var mtObjectType = uow.MTObjectRepository.GetQuery().SingleOrDefault(a => a.MT_FieldType.Id == curMTObjectTypeField.Id);
            var results = new List<T>();

            if (mtObjectType == null)
            {
                return results;
            }

            var fieldList = new List<Entities.MT_Field>();
            var fieldMapping = GetFieldMappingForObject(uow, mtObjectType.Id, fieldList);
            var exprTransformation = new ExpressionTransformation<T, Entities.MT_Data>(x => fieldMapping[x]);

            mtObjectType.Fields = fieldList;

            exprTransformation.Parse(query);

            // we should try to do queries on the DB side in future
            foreach (var dataRow in uow.MTDataRepository.GetQuery().Where(x => x.MT_ObjectId == mtObjectType.Id))
            {
                if (!exprTransformation.CompiledTargetExpression(dataRow))
                {
                    continue;
                }

                results.Add(MapMTDataToManifest<T>(dataRow, mtObjectType));
            }

            return results;
        }
        
        private static Dictionary<string, string> GetFieldMappingForObject(IUnitOfWork uow, int mtObjectId, List<Entities.MT_Field> fieldList)
        {
            var fields = new Dictionary<string, string>();

            foreach (var field in uow.MTFieldRepository.GetQuery().Where(x => x.MT_ObjectId == mtObjectId))
            {
                var alias = "Value" + field.FieldColumnOffset;
                string existingAlias;

                if (fields.TryGetValue(field.Name, out existingAlias))
                {
                    if (existingAlias != alias)
                    {
                        throw new InvalidOperationException(string.Format("Duplicate field definition. MT object type: {0}. Field {1} is mapped to {2} and {3}", mtObjectId, field.Name, existingAlias, alias));
                    }
                }
                else
                {
                    fields[field.Name] = alias;
                    fieldList.Add(field);
                }
            }
            
            return fields;
        }

        private Entities.MT_Data FindMT_DataByExpression(IUnitOfWork _uow, string curFr8AccountId, PropertyInfo leftOperand, object rightOperand, MethodInfo equalMethod, Entities.MT_Object curMTObject)
        {
            Entities.MT_Data curMTData = null;
            if (curMTObject != null)
            {
                var correspondingDTFields = _uow.MTFieldRepository.FindList(a => a.MT_ObjectId == curMTObject.Id);
                var keyMTField = correspondingDTFields.FirstOrDefault(a => a.Name == leftOperand.Name);
                var corrMTDataProperty = typeof(Entities.MT_Data).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(a => a.Name == "Value" + keyMTField.FieldColumnOffset);

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

        private Entities.MT_Data FindMT_DataByKeyField<T>(IUnitOfWork _uow, string curFr8AccountId, T curManifest, Entities.MT_Object correspondingDTObject, IEnumerable<Entities.MT_Field> correspondingDTFields, PropertyInfo keyPropertyInfo) where T : Manifest
        {
            Entities.MT_Data correspondingMTData = null;
            var keyValue = keyPropertyInfo.GetValue(curManifest);
            var possibleDatas = _uow.MTDataRepository.FindList(a => a.MT_ObjectId == correspondingDTObject.Id && a.fr8AccountId == curFr8AccountId);
            var keyMTField = correspondingDTFields.FirstOrDefault(a => a.Name == keyPropertyInfo.Name);
            correspondingMTData = null;
            var corrMTDataProperty = typeof(Entities.MT_Data).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(a => a.Name == "Value" + keyMTField.FieldColumnOffset);

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

        private Entities.MT_Object FindMT_ObjectByExpression<T>(IUnitOfWork _uow, Expression<Func<T, object>> conditionOnKeyProperty, int ManifestId, out PropertyInfo leftOperand, out object rightOperand, out MethodInfo equalMethod) where T : Manifest
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
            Entities.MT_Object curMTObject = null;
            if (ManifestId == -1)
            {
                //We assume that there must be a single MTObject                
                curMTObject = _uow.MTObjectRepository.GetQuery().SingleOrDefault(a => a.MT_FieldType.Id == curMTObjectTypeField.Id);
                if (curMTObject != null)
                    curMTObject.Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == curMTObject.Id).ToList();
            }
            else
            {
                curMTObject = _uow.MTObjectRepository.GetQuery().SingleOrDefault(a => a.MT_FieldType.TypeName == curMTObjectTypeField.TypeName && a.ManifestId == ManifestId);
                if (curMTObject != null)
                    curMTObject.Fields = _uow.MTFieldRepository.GetQuery().Where(a => a.MT_ObjectId == curMTObject.Id).ToList();
            }

            return curMTObject;
        }

        //maps BaseMTO to MTData
        private void MapManifestToMTData(
            string curFr8AccountId,
            Manifest curManifest,
            List<PropertyInfo> curDataProperties,
            Entities.MT_Data data,
            Entities.MT_Object correspondingDTObject)
        {
            var correspondingDTFields = correspondingDTObject.Fields;
            data.fr8AccountId = curFr8AccountId;
            data.GUID = Guid.Empty;
            data.MT_ObjectId = correspondingDTObject.Id;

            var dataValueCells = data.GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .ToList();

            foreach (var field in correspondingDTFields)
            {
                var property = curDataProperties
                    .FirstOrDefault(a => a.Name == field.Name);

                var corrDataCell = dataValueCells
                    .FirstOrDefault(a => a.Name == "Value" + field.FieldColumnOffset);

                var val = property.GetValue(curManifest);

                if (IsOfPrimitiveType(val))
                {
                    corrDataCell.SetValue(data, Convert.ChangeType(val, typeof(string), CultureInfo.InvariantCulture));
                }
                else
                {
                    corrDataCell.SetValue(data, ConvertValueToJson(val));
                }
            }
        }

        private string ConvertValueToJson(object val)
        {
            return JsonConvert.SerializeObject(val);
        }

        private bool IsOfPrimitiveType(object val)
        {
            if (val == null)
            {
                return true;
            }

            return IsOfPrimitiveType(val.GetType());
        }

        private bool IsOfPrimitiveType(Type type)
        {
            return PrimitiveTypes.Contains(type);
        }

        private static bool TryGetNullableType(Type orignalType, out Type underlyingType)
        {
            if (orignalType.IsGenericType && orignalType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                underlyingType = orignalType.GetGenericArguments()[0];
                return true;
            }

            underlyingType = orignalType;
            return false;
        }

        //instantiate object from MTData
        private T MapMTDataToManifest<T>(Entities.MT_Data data, Entities.MT_Object correspondingDTObject)
        {
            var correspondingDTFields = correspondingDTObject.Fields;
            var objMTType = correspondingDTObject.MT_FieldType;
            object obj = Activator.CreateInstance(objMTType.AssemblyName, objMTType.TypeName).Unwrap();
            var properties = obj.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
            var dataValueCells = data.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();

            if (correspondingDTFields != null)
            {
                foreach (var dtField in correspondingDTFields)
                {
                    var correspondingProperty = properties.FirstOrDefault(a => a.Name == dtField.Name);
                    var valueCell = dataValueCells.FirstOrDefault(a => a.Name == "Value" + dtField.FieldColumnOffset);
                    Type manifestPropType;

                    if (valueCell == null || correspondingProperty == null)
                    {
                        continue;
                    }

                    bool isNullable = TryGetNullableType(correspondingProperty.PropertyType, out manifestPropType);
                    var val = valueCell.GetValue(data);

                    if (IsOfPrimitiveType(manifestPropType))
                    {
                        if (val != null)
                        {
                            val = Convert.ChangeType(val, manifestPropType, CultureInfo.InvariantCulture);

                            if (isNullable)
                            {
                                val = Activator.CreateInstance(typeof (Nullable<>).MakeGenericType(manifestPropType), val);
                            }
                        }

                        correspondingProperty.SetValue(obj, val);
                    }
                    else
                    {
                        correspondingProperty.SetValue(
                            obj,
                            ConvertValueFromJson(correspondingProperty.PropertyType, val)
                            );
                    }
                }
            }

            return (T) obj;
        }

        private object ConvertValueFromJson(Type type, object sourceValue)
        {
            if (sourceValue.GetType() != typeof(string))
            {
                throw new ApplicationException("SourceValue is not a string");
            }

            return JsonConvert.DeserializeObject((string)sourceValue, type);
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            var propInfo = member.Member as PropertyInfo;
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