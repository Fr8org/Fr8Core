using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Infrastructure.StructureMap
{
    public abstract class MockedDbSet : IEnumerable
    {
        public abstract void Save();
        public abstract IEnumerable LocalEnumerable { get; }
        public abstract IEnumerator GetEnumerator();
        public abstract void WipeDatabase();
    }

    public class MockedDbSet<TEntityType> : MockedDbSet, 
        IDbSet<TEntityType>
        where TEntityType : class
    {
        private static readonly HashSet<TEntityType> SavedSet = new HashSet<TEntityType>();
        private readonly HashSet<TEntityType> _rowsToDelete = new HashSet<TEntityType>(); 
        private readonly HashSet<TEntityType> _set = new HashSet<TEntityType>();
        private readonly IEnumerable<MockedDbSet> _subSets;

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
            if (keyType != typeof(int) && keyType != typeof(String))
                throw new Exception("Only supports int-based or string-based keys.");

            if (keyType == typeof(String))
            {
                string entityPrimaryKey = keyValues[0] as string;
                Func<TEntityType, string> compiledSelector = GetEntityKeySelectorString().Compile();
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

        public override void Save()
        {
            foreach (var set in _set)
            {
                if (!SavedSet.Contains(set))
                    SavedSet.Add(set);
            }

            foreach (var rowToDelete in _rowsToDelete)
            {
                if (SavedSet.Contains(rowToDelete))
                    SavedSet.Remove(rowToDelete);
            }
        }

        public override IEnumerable LocalEnumerable
        {
            get { return Local; }
        }
    }
}
