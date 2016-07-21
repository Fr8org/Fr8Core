using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Entities;
using Data.Interfaces;
using Data.Migrations;
using Data.States.Templates;
using Fr8.Infrastructure.Utilities;
using Segment.Model;
using StructureMap;

namespace Data.Infrastructure.StructureMap
{
    public class MockedDBContext : IDBContext
    {
        private static readonly List<MockedDbSet> _SetsToClear = new List<MockedDbSet>();
        public static void WipeMockedDatabase()
        {
            lock (_SetsToClear)
            {
                foreach (var set in _SetsToClear)
                {
                    set.WipeDatabase();
                }
            }
        }

        public MockedDBContext()
        {
            SetUnique<EmailAddressDO, String>(ea => ea.Address);
            SetUnique<Fr8AccountDO, int?>(u => u.EmailAddressID);

            SetPrimaryKey<Fr8AccountDO, String>(u => u.Id);

            MigrationConfiguration.SeedIntoMockDb(new UnitOfWork(this, ObjectFactory.Container));
        }

        private readonly Dictionary<Type, PropertyInfo> _forcedDOPrimaryKey = new Dictionary<Type, PropertyInfo>();

        private void SetPrimaryKey<TOnType, TReturnType>(Expression<Func<TOnType, TReturnType>> expression)
        {
            var reflectionHelper = new ReflectionHelper<TOnType>();
            var propName = reflectionHelper.GetPropertyName(expression);
            var linkedProp = typeof(TOnType).GetProperties().FirstOrDefault(p => p.Name == propName);
            lock (_forcedDOPrimaryKey)
                _forcedDOPrimaryKey[typeof(TOnType)] = linkedProp;
        }

        private readonly Dictionary<Type, List<String>> _uniqueProperties = new Dictionary<Type, List<String>>();
        private void SetUnique<TOnType, TReturnType>(Expression<Func<TOnType, TReturnType>> expression)
        {
            var reflectionHelper = new ReflectionHelper<TOnType>();
            var propName = reflectionHelper.GetPropertyName(expression);
            lock (_uniqueProperties)
            {
                if (!_uniqueProperties.ContainsKey(typeof(TOnType)))
                    _uniqueProperties[typeof(TOnType)] = new List<String>();

                _uniqueProperties[typeof(TOnType)].Add(propName);
            }
        }

        private readonly Dictionary<Type, MockedDbSet> _cachedSets = new Dictionary<Type, MockedDbSet>();

        private object[] _addedEntities;
        private object[] _deletedEntities;
        private object[] _modifiedEntities;

        public int SaveChanges()
        {
            AddForeignRows();

            var adds = GetAdds().ToList();
            var update = GetUpdates().ToList();
            var createdEntityList = adds.OfType<ICreateHook>().ToList();

            foreach (var createdEntity in createdEntityList)
            {
                createdEntity.BeforeCreate();
            }

            SaveSets();

            DetectChanges();


            var changed = adds.Union(update).ToArray();

            AssignIDs(changed);
            UpdateForeignKeyReferences(changed);

            AssertConstraints();

            foreach (var createdEntity in createdEntityList)
            {
                createdEntity.AfterCreate();
            }

            return 1;
        }

        private void SaveSets()
        {
            lock (_cachedSets)
            {
                foreach (var cachedSet in _cachedSets)
                {
                    var set = cachedSet.Value;
                    set.Save();
                }
            }
        }

        public void DetectChanges()
        {
            _addedEntities = GetAdds().ToArray();
            _deletedEntities = GetDeletes().ToArray();
            _modifiedEntities = GetUpdates().ToArray();
        }


        public object[] AddedEntities
        {
            get { return _addedEntities; }
        }

        public object[] ModifiedEntities
        {
            get
            {
                return _modifiedEntities;
            }
        }

        public object[] DeletedEntities
        {
            get
            {
                return _deletedEntities;
            }
        }

        private void AssertConstraints()
        {
            lock (_cachedSets)
            {
                foreach (var set in _cachedSets.ToList())
                {
                    foreach (object row in set.Value)
                    {
                        foreach (var prop in row.GetType().GetProperties())
                        {
                            var defaultAttribute = prop.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault();
                            if (defaultAttribute != null)
                            {
                                if (prop.GetValue(row) == null)
                                {
                                    prop.SetValue(row, defaultAttribute.Value);
                                }
                            }

                            //Check nullable constraint enforced
                            var hasAttribute = prop.GetCustomAttributes(typeof(RequiredAttribute)).Any();
                            if (hasAttribute)
                            {
                                if (prop.GetValue(row) == null)
                                    throw new Exception("Property '" + prop.Name + "' on '" + row.GetType().Name + "' is marked as required, but is being saved with a null value.");
                            }
                        }
                    }
                }
            }

            lock (_uniqueProperties)
            {
                foreach (var kvp in _uniqueProperties)
                {
                    var set = Set(kvp.Key);

                    foreach (var uniqueProperty in kvp.Value)
                    {
                        var propName = uniqueProperty;
                        var param = Expression.Parameter(kvp.Key);
                        var propExpr = Expression.Property(param, propName);
                        var fullExpr = Expression.Lambda(propExpr, param);
                        var compiled = fullExpr.Compile();

                        var hashSet = new HashSet<dynamic>();
                        foreach (var row in set)
                        {
                            var value = compiled.DynamicInvoke(row);
                            if (hashSet.Contains(value))
                                throw new Exception(String.Format("Duplicate values for '{0}' on '{1}' are not allowed. Duplicated value: '{2}'", propName, kvp.Key.Name, value));
                            hashSet.Add(value);
                        }
                    }
                }
            }
        }

        private IEnumerable<object> GetAdds()
        {
            lock (_cachedSets)
            {
                var returnSet = new List<object>();
                var sets = _cachedSets.ToList();
                foreach (var set in sets)
                {
                    returnSet.AddRange(set.Value.LocalEnumerable.Cast<object>().Where(row => !set.Value.OfType<object>().Contains(row)));
                }
                return returnSet;
            }
        }


        private IEnumerable<object> GetDeletes()
        {
            lock (_cachedSets)
            {
                var returnSet = new List<object>();
                foreach (var set in _cachedSets)
                {
                    returnSet.AddRange(set.Value.DeletedEntries.OfType<object>());
                }

                return returnSet;
            }
        }
        
        private IEnumerable<object> GetUpdates()
        {
            lock (_cachedSets)
            {
                var returnSet = new List<object>();
                foreach (var set in _cachedSets)
                {
                    returnSet.AddRange(set.Value.ModifiedEntries.OfType<object>());
                }

                return returnSet;
            }
        }


        private void AssignIDs(IEnumerable<object> adds)
        {
            lock (_cachedSets)
            {
                foreach (var grouping in adds.GroupBy(a => a.GetType()))
                {
                    int maxIDAlready = 0;
                    var savedCollection = Set(grouping.Key).OfType<object>().ToList();
                    if (savedCollection.Any())
                    {
                        var propertyInfo = EntityPrimaryKeyPropertyInfo(grouping.Key);
                        if (propertyInfo == null || propertyInfo.PropertyType != typeof(int))
                            continue;

                        maxIDAlready = savedCollection.Max<object, int>(a => (int)propertyInfo.GetValue(a));
                    }
                    foreach (var row in grouping)
                    {
                        var propInfo = EntityPrimaryKeyPropertyInfo(row);
                        if (propInfo == null)
                            continue;

                        if (propInfo.PropertyType == typeof(int)
                            && (int)propInfo.GetValue(row) == 0)
                        {
                            propInfo.SetValue(row, ++maxIDAlready);
                        }
                    }
                }
            }
        }

        // hardcode for handling Id <-> Fk synchronization in case of our enum properties that has DB 'templates' (IStateTemplate).
        private static bool IsStateTemplateProperty(PropertyInfo fkId, PropertyInfo fkDo)
        {
            return typeof (IStateTemplate).IsAssignableFrom(fkDo.PropertyType);
        }

        private void UpdateForeignKeyReferences(IEnumerable<object> newRows)
        {
            foreach (var grouping in newRows.GroupBy(r => r.GetType()))
            {
                if (!grouping.Any())
                    continue;

                var propType = grouping.Key;
                var props = propType.GetProperties();
                var propsWithForeignKeyNotation = props.Where(p => p.GetCustomAttribute<ForeignKeyAttribute>(true) != null).ToList();
                if (!propsWithForeignKeyNotation.Any())
                    continue;

                foreach (var prop in propsWithForeignKeyNotation)
                {
                    var attr = prop.GetCustomAttribute<ForeignKeyAttribute>(true);
                    //Now.. find out which way it goes..

                    var linkedName = attr.Name;
                    var linkedProp = propType.GetProperties().FirstOrDefault(n => n.Name == linkedName);
                    if (linkedProp == null)
                        continue;

                    PropertyInfo foreignIDProperty;
                    PropertyInfo parentFKIDProperty;
                    PropertyInfo parentFKDOProperty;

                    var linkedID = EntityPrimaryKeyPropertyInfo(linkedProp.PropertyType);
                    var foreignType = linkedProp.PropertyType;
                    if (linkedID != null)
                    {
                        foreignIDProperty = linkedID;
                        parentFKIDProperty = prop;
                        parentFKDOProperty = linkedProp;
                    }
                    else
                    {
                        foreignIDProperty = EntityPrimaryKeyPropertyInfo(prop.PropertyType);
                        foreignType = prop.PropertyType;
                        parentFKIDProperty = linkedProp;
                        parentFKDOProperty = prop;
                    }

                    var foreignCollectionProps = foreignType.GetProperties()
                        .Where(p => p.PropertyType.IsGenericType &&
                                    typeof(IList<>).MakeGenericType(propType).IsAssignableFrom(p.PropertyType) &&
                                    p.PropertyType.GetGenericArguments()[0] == propType).ToList();

                    if (foreignIDProperty == null)
                        continue;

                    foreach (var value in grouping)
                    {
                        var foreignDO = parentFKDOProperty.GetValue(value);
                        // In case of StateTemplates we never updates DO, so Id has more priority
                        if (foreignDO != null && !IsStateTemplateProperty(foreignIDProperty, parentFKDOProperty)) //If the DO is set, then we update the ID
                        {
                            var fkID = foreignIDProperty.GetValue(foreignDO);
                            parentFKIDProperty.SetValue(value, fkID);
                        }
                        else
                        {
                            var fkID = parentFKIDProperty.GetValue(value);
                            if (fkID == null)
                                continue;

                            // TODO: @yakov.gnusin: here, wrong _cachedSet for ActivityDO.
                            var foreignSet = Set(parentFKDOProperty.PropertyType);
                            foreach (var foreignRow in foreignSet)
                            {
                                var id = foreignIDProperty.GetValue(foreignRow);
                                if (id.Equals(fkID))
                                {
                                    foreignDO = foreignRow;
                                    break;
                                }
                            }
                            if (foreignDO == null)
                                throw new Exception(String.Format("Foreign row does not exist.\nValue '{0}' on '{1}.{2}' pointing to '{3}.{4}'", fkID, grouping.Key.Name, parentFKIDProperty.Name, parentFKDOProperty.PropertyType.Name, foreignIDProperty.Name));

                            parentFKDOProperty.SetValue(value, foreignDO);
                        }

                        //Now we add ourselves to their collection (if they have one)
                        foreach (var foreignCollectionProp in foreignCollectionProps)
                        {
                            var collectionToAddTo = foreignCollectionProp.GetValue(foreignDO) as IList;
                            if (collectionToAddTo == null)
                                continue;

                            if (!collectionToAddTo.Contains(value))
                                collectionToAddTo.Add(value);
                        }
                    }
                }
            }
        }

        private void AddForeignRows()
        {
            List<Object> currentPass;
            lock (_cachedSets)
            {
                currentPass = new List<object>(_cachedSets.SelectMany(c => c.Value.LocalEnumerable.OfType<object>()));
                currentPass.AddRange(_cachedSets.SelectMany(c => c.Value.ModifiedEntries.OfType<object>()));
            }

            var nextPass = new List<Object>();

            while (currentPass.Any())
            {
                foreach (var row in currentPass)
                {
                    PropertyInfo[] props = row.GetType().GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        Type castValue = null;
                        var actualValue = prop.GetValue(row);
                        if (actualValue != null)
                            castValue = actualValue.GetType();
                        if (IsEntity(prop.PropertyType) || (castValue != null && IsEntity(castValue)))
                        {
                            //It's a normal foreign key
                            object value = prop.GetValue(row);
                            if (value == null)
                                continue;

                            if (AddValueToForeignSet(value))
                                nextPass.Add(value);
                        }
                        else if (prop.PropertyType.IsGenericType &&
                                 typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                                 IsEntity(prop.PropertyType.GetGenericArguments()[0]))
                        {
                            //It's a collection!
                            IEnumerable collection = prop.GetValue(row) as IEnumerable;
                            if (collection == null)
                                continue;

                            nextPass.AddRange(collection.OfType<object>().Where(AddValueToForeignSet));
                        }
                    }
                }
                currentPass = new List<object>(nextPass);
                nextPass.Clear();
            }
        }

        private bool AddValueToForeignSet(Object value)
        {
            if (value.GetType().IsNested)
                return false;

            var checkSet = Set(value.GetType());
            if (checkSet.OfType<object>().Union(checkSet.LocalEnumerable.OfType<object>()).Contains(value))
                return false;

            MethodInfo methodToCall = checkSet.GetType().GetMethod("Add", new[] { value.GetType() });
            methodToCall.Invoke(checkSet, new[] { value });
            return true;
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            Type entityType = typeof(TEntity);
            return (IDbSet<TEntity>)(Set(entityType));
        }

        private MockedDbSet Set(Type entityType)
        {
            lock (_cachedSets)
            {
                if (!_cachedSets.ContainsKey(entityType))
                {
                    var assemblyTypes = entityType.Assembly.GetTypes();
                    var subclassedSets = assemblyTypes.Where(a => a.IsSubclassOf(entityType) && entityType != a).ToList();
                    var otherSets = subclassedSets.Select(Set);

                    _cachedSets[entityType] = (MockedDbSet)Activator.CreateInstance(typeof(MockedDbSet<>).MakeGenericType(entityType), otherSets);
                }

                var returnSet = _cachedSets[entityType];
                lock (_SetsToClear)
                {
                    if (!_SetsToClear.Contains(returnSet))
                        _SetsToClear.Add(returnSet);
                }
                return returnSet;
            }
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork UnitOfWork { get; set; }

        private bool IsEntity(Type type)
        {
            return type.IsClass && !string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith("Data.Entities");
        }

        public PropertyInfo EntityPrimaryKeyPropertyInfo(object entity)
        {
            var entityType = entity.GetType();
            return EntityPrimaryKeyPropertyInfo(entityType);
        }

        public PropertyInfo EntityPrimaryKeyPropertyInfo(Type entityType)
        {
            lock (_forcedDOPrimaryKey)
            {
                if (_forcedDOPrimaryKey.ContainsKey(entityType))
                    return _forcedDOPrimaryKey[entityType];
            }
            return
                ReflectionHelper.EntityPrimaryKeyPropertyInfo(entityType);
        }

        public void Dispose()
        {

        }
    }
}
