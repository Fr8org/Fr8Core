using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Data.Entities;

namespace Data.Infrastructure.StructureMap
{
    public abstract class MockedDbSet : IEnumerable
    {
        internal abstract void Save();
        public abstract IEnumerable DeletedEntries { get; }
        public abstract IEnumerable ModifiedEntries { get; }
        public abstract IEnumerable LocalEnumerable { get; }
        public abstract IEnumerator GetEnumerator();
        public abstract void WipeDatabase();
    }

    public class MockedDbSet<TEntityType> : MockedDbSet, 
        IDbSet<TEntityType>
        where TEntityType : class
    {
        private static readonly HashSet<TEntityType> SavedSet = new HashSet<TEntityType>();
        private static readonly Dictionary<TEntityType, string> Digests = new Dictionary<TEntityType, string>();
        private readonly HashSet<TEntityType> _rowsToDelete = new HashSet<TEntityType>(); 
        private readonly HashSet<TEntityType> _set = new HashSet<TEntityType>();
        private readonly IEnumerable<MockedDbSet> _subSets;
        private static readonly List<Func<TEntityType, object>> DigestProperties;


        static MockedDbSet()
        {
            DigestProperties = new List<Func<TEntityType, object>>();
            
            foreach (var prop in typeof(TEntityType).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue;
                }

                if (IsTypeSuitableForDigest(prop.PropertyType) && prop.CanRead)
                {
                    var p = prop;
                    DigestProperties.Add(x => p.GetValue(x));
                }
            }
        }

        private static bool IsTypeSuitableForDigest(Type type)
        {
            if (type  == typeof(string) || type.IsValueType)
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                return IsTypeSuitableForDigest(type.GetGenericArguments()[0]);
            }

            return false;
        }

        public override void WipeDatabase()
        {
            SavedSet.Clear();
        }
        
        private IEnumerable<TEntityType> LocalSet
        {
            get
            {
                foreach (var val in _set)
                    yield return val;
                foreach (var subSet in _subSets)
                    foreach (var val in subSet.LocalEnumerable.OfType<TEntityType>())
                        yield return val;
            }
        }

        public override IEnumerable ModifiedEntries
        {
            get
            {
                if (typeof (AuthorizationTokenDO) == typeof (TEntityType))
                {
                    int wtf = 0;
                }
                foreach (var entry in SavedSet)
                {
                    if (_rowsToDelete.Contains(entry))
                    {
                        yield break;
                    }

                    string digest;

                    Digests.TryGetValue(entry, out digest);

                    if (digest != ComputeEntityDigest(entry))
                    {
                        yield return entry;
                    }
                }
            }
        }

        public override IEnumerable DeletedEntries
        {
            get { return _rowsToDelete; }
        }

        private IEnumerable<TEntityType> MergedSets
        {
            get
            {
                foreach (var val in SavedSet)
                    yield return val;
                foreach(var subSet in _subSets)
                    foreach (var val in subSet.OfType<TEntityType>())
                        yield return val;
            }
        }

        public MockedDbSet(IEnumerable<MockedDbSet> subSets)
        {
            _subSets = subSets;
            _set = new HashSet<TEntityType>();
        }
        
        public override IEnumerator GetEnumerator()
        {
            return (this as IEnumerable<TEntityType>).GetEnumerator();
        }

        IEnumerator<TEntityType> IEnumerable<TEntityType>.GetEnumerator()
        {
            return MergedSets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get
            {
                return MergedSets.AsQueryable().Expression;
            }
        }

        public Type ElementType
        {
            get
            {
                return MergedSets.AsQueryable().ElementType;
            }
        }
        public IQueryProvider Provider
        {
            get
            {
                return MergedSets.AsQueryable().Provider;
            }
        }

        public TEntityType Find(params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new Exception("No primary key provided for " + EntityName);
            if (keyValues.Length > 1)
                throw new Exception("Multiple keys provided for " + EntityName + ". Only singular keys are supported");

            var keyType = keyValues[0].GetType();
            if (keyType != typeof(int) && keyType != typeof(String) && keyType != typeof(Guid))
            {
                throw new Exception("Only supports guid-base, int-based or string-based keys.");
            }

            if (keyType == typeof(String))
            {
                string entityPrimaryKey = keyValues[0] as string;
                Func<TEntityType, string> compiledSelector = GetEntityKeySelectorString().Compile();
                var isSaved = SavedSet.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
                if (isSaved != null)
                    return isSaved;
                return LocalSet.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
            }
            else if (keyType == typeof(Guid))
            {
                var entityPrimaryKey = (Guid)(keyValues[0]);
                Func<TEntityType, Guid> compiledSelector = GetEntityKeySelectorGuid().Compile();
                var isSaved = SavedSet.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
                if (isSaved != null)
                    return isSaved;
                return LocalSet.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
            }
            else
            {

                int entityPrimaryKey = (int)(keyValues[0]);
                Func<TEntityType, int> compiledSelector = GetEntityKeySelectorInt().Compile();
                var isSaved = SavedSet.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
                if (isSaved != null)
                    return isSaved;
                return LocalSet.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
            }
        }

        public TEntityType Add(TEntityType entity)
        {
            _set.Add(entity);
            return entity;
        }

        public TEntityType Remove(TEntityType entity)
        {
            _set.Remove(entity);
            if (!_rowsToDelete.Contains(entity))
                _rowsToDelete.Add(entity);
            return entity;
        }

        public TEntityType Attach(TEntityType entity)
        {
            return entity;
        }

        public TEntityType Create()
        {
            throw new Exception("Not supported yet!");
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntityType
        {
            throw new Exception("Not supported yet!");
        }

        public ObservableCollection<TEntityType> Local
        {
            get
            {
                return new ObservableCollection<TEntityType>(LocalSet);
            }
        }

        protected Expression<Func<TEntityType, int>> GetEntityKeySelectorInt()
        {
            Type entityType = typeof(TEntityType);
            PropertyInfo tableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression proe = Expression.Parameter(entityType);
            MemberExpression propertyAccessor = Expression.Property(proe, tableKey);
            Expression<Func<TEntityType, int>> entityKeySelector = Expression.Lambda(propertyAccessor, new[] { proe }) as Expression<Func<TEntityType, int>>;
            return entityKeySelector;
        }

        protected Expression<Func<TEntityType, String>> GetEntityKeySelectorString()
        {
            Type entityType = typeof(TEntityType);
            PropertyInfo tableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression proe = Expression.Parameter(entityType);
            MemberExpression propertyAccessor = Expression.Property(proe, tableKey);
            Expression<Func<TEntityType, String>> entityKeySelector = Expression.Lambda(propertyAccessor, new[] { proe }) as Expression<Func<TEntityType, String>>;
            return entityKeySelector;
        }

        protected Expression<Func<TEntityType, Guid>> GetEntityKeySelectorGuid()
        {
            Type entityType = typeof(TEntityType);
            PropertyInfo tableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression proe = Expression.Parameter(entityType);
            MemberExpression propertyAccessor = Expression.Property(proe, tableKey);
            Expression<Func<TEntityType, Guid>> entityKeySelector = Expression.Lambda(propertyAccessor, new[] { proe }) as Expression<Func<TEntityType, Guid>>;
            return entityKeySelector;
        }


        private String _entityName;
        public String EntityName
        {
            get
            {
                return _entityName ?? (_entityName = typeof(TEntityType).Name);
            }
        }

        private PropertyInfo _entityPrimaryKeyPropertyInfo;
        public PropertyInfo EntityPrimaryKeyPropertyInfo
        {
            get
            {
                if (_entityPrimaryKeyPropertyInfo == null)
                {
                    List<PropertyInfo> keys = typeof(TEntityType).GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any()).ToList();
                    if (!keys.Any())
                        keys = typeof(TEntityType).GetProperties().Where(p => p.Name == "Id").ToList();

                    if (keys.Count > 1)
                        throw new Exception("Entity MUST have a single primary key. Composite keys are not supported.");
                    //If no primary key exists, we cannot use it
                    if (keys.Count == 0)
                        throw new Exception("Entity MUST have a single primary key. Entities without primary keys are not supported.");

                    _entityPrimaryKeyPropertyInfo = keys.First();
                }
                return _entityPrimaryKeyPropertyInfo;
            }
        }

        public void AddOrUpdate(
            Expression<Func<TEntityType, Object>> identifierExpression,
            params TEntityType[] entities
            )
        {
            var lambda = identifierExpression.Compile();
            foreach (var entity in entities)
            {
                if (!this.Any(dbEntity => lambda(dbEntity).Equals(lambda(entity))))
                {
                    Add(entity);
                }
            }
        }

        private string ComputeEntityDigest(TEntityType entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            foreach (var prop in DigestProperties)
            {
                sb.Append('|');
                sb.Append(prop(entity));
            }

            return sb.ToString();
        }

        internal override void Save()
        {
            foreach (var entity in _set)
            {
                if (!SavedSet.Contains(entity))
                {
                    SavedSet.Add(entity);
                    Digests[entity] = ComputeEntityDigest(entity);
                }
                else
                {
                    if (!_rowsToDelete.Contains(entity))
                    {
                        Digests[entity] = ComputeEntityDigest(entity);
                    }
                }
            }

            foreach (var rowToDelete in _rowsToDelete)
            {
                if (SavedSet.Contains(rowToDelete))
                {
                    SavedSet.Remove(rowToDelete);
                    Digests.Remove(rowToDelete);
                }
            }

            _rowsToDelete.Clear();
        }

        public override IEnumerable LocalEnumerable
        {
            get { return Local; }
        }
    }
}
