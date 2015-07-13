using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Interfaces;
using Data.States;

namespace Data.Entities
{
    /// <summary>
    /// This class is used to manage CustomFields linked to Entities. See Core.Services.TrackingStatus for more information.
    /// It's a generic implementation, and as such, can be used with any entity in the database, so long as it has a single primary key. Composite keys are not supported.
    /// </summary>
    /// <typeparam name="TForeignEntity">The type of the linked entity (<see cref="EmailDO"></see>, for example)</typeparam>
    /// <typeparam name="TCustomFieldType">The type of the custom field (<see cref="TrackingState"></see> for example</typeparam>
    public class GenericCustomField<TCustomFieldType, TForeignEntity>
        where TCustomFieldType : class, ICustomField, new()
        where TForeignEntity : class, new()
    {
        private readonly IGenericRepository<TCustomFieldType> _trackingStatusRepo;
        private readonly IGenericRepository<TForeignEntity> _foreignRepo;

        public GenericCustomField(IGenericRepository<TCustomFieldType> trackingStatusRepo,
            IGenericRepository<TForeignEntity> foreignRepo)
        {
            _trackingStatusRepo = trackingStatusRepo;
            _foreignRepo = foreignRepo;
        }

        /// <summary>
        /// Get all entities without a custom field
        /// </summary>09
        /// <returns>IQueryable of entities without any custom field</returns>
        protected IQueryable<TForeignEntity> GetEntitiesWithoutCustomFields(Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            return GetEntities(customFieldPredicate, null, jr => jr.CustomFieldDO == null);
        }

        /// <summary>
        /// Get all entities with a custom field
        /// </summary>
        /// <returns>IQueryable of entities with a custom field</returns>
        protected IQueryable<TForeignEntity> GetEntitiesWithCustomField(Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            return GetEntities(customFieldPredicate);
        }

        /// <summary>
        /// Gets the custom field of an entity. If a custom field does not exist, it will be created.
        /// Entities _MUST_ already exist in the database.
        /// </summary>
        /// <param name="entityDO">Entity to set the custom field on</param>
        /// <param name="customFieldPredicate"></param>
        protected TCustomFieldType GetOrCreateCustomField(TForeignEntity entityDO, Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            return GetOrCreateCustomField(GetKey(entityDO), customFieldPredicate);
        }

        private TCustomFieldType GetOrCreateCustomField(int entityID, Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            TCustomFieldType currentStatus = GetCustomField(entityID, customFieldPredicate);
            if (currentStatus == null)
            {
                currentStatus = new TCustomFieldType
                {
                    Id = entityID,
                    ForeignTableName = EntityName,
                };
                _trackingStatusRepo.Add(currentStatus);
            }
            return currentStatus;
        }

        /// <summary>
        /// Deletes the custom field of an entity. If no custom field exists, no action will be performed.
        /// </summary>
        /// <param name="entityDO">Entity to delete the custom field on</param>
        /// <param name="customFieldPredicate"></param>
        protected void DeleteCustomField(TForeignEntity entityDO, Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            DeleteCustomField(GetKey(entityDO), customFieldPredicate);
        }

        protected void DeleteCustomField(int entityID, Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            TCustomFieldType currentStatus = GetCustomField(entityID, customFieldPredicate);
            if (currentStatus != null)
            {
                _trackingStatusRepo.Remove(currentStatus);
            }
        }

        /// <summary>
        /// Gets the current custom field of an entity. If no status exists, null will be returned.
        /// </summary>
        /// <param name="entityDO">The custom field of the provided entity</param>
        /// <param name="customFieldPredicate"></param>
        protected TCustomFieldType GetCustomField(TForeignEntity entityDO, Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            int inMemoryID = GetKey(entityDO);
            return GetCustomField(inMemoryID, customFieldPredicate);
        }

        protected TCustomFieldType GetCustomField(int entityID, Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null)
        {
            if (customFieldPredicate == null)
                customFieldPredicate = cf => true;

            return GetCustomFields(entityID).Where(customFieldPredicate).FirstOrDefault();
        }

        protected IQueryable<TCustomFieldType> GetCustomFields(int entityID)
        {
            //This effectively builds a lambda as follows:
            // e => e.[PrimaryKeyProperty] == entityID

            Type foreignTableType = typeof(TForeignEntity);
            PropertyInfo foreignTableKey = EntityPrimaryKeyPropertyInfo;

            ParameterExpression foreignProp = Expression.Parameter(foreignTableType);
            MemberExpression propertyAccessor = Expression.Property(foreignProp, foreignTableKey);

            BinaryExpression equalExpression = Expression.Equal(propertyAccessor, Expression.Constant(entityID));
            Expression<Func<TForeignEntity, bool>> foreignKeyComparer = Expression.Lambda(equalExpression, new[] { foreignProp }) as Expression<Func<TForeignEntity, bool>>;

            return GetJoinResult(null, foreignKeyComparer).Select(jr => jr.CustomFieldDO);
        }


        protected IQueryable<TForeignEntity> GetEntities(
            Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null,
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate = null,
            Expression<Func<JoinResult, bool>> joinPredicate = null)
        {
            return GetJoinResult(customFieldPredicate, foreignEntityPredicate, joinPredicate)
                .Select(a => a.ForeignDO);
        }

        protected IQueryable<JoinResult> GetJoinResult(
            Expression<Func<TCustomFieldType, bool>> customFieldPredicate = null,
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate = null,
            Expression<Func<JoinResult, bool>> joinPredicate = null)
        {
            //If we don't have a predicate on the tracking status, set it to always true (Linq2SQL removes the redundant 'return true' predicate, so no performance is lost)
            if (customFieldPredicate == null)
                customFieldPredicate = customFieldDO => true;

            //If we don't have a predicate on the foreign entity, set it to always true (Linq2SQL removes the redundant 'return true' predicate, so no performance is lost)
            if (foreignEntityPredicate == null)
                foreignEntityPredicate = foreignEntityDO => true;

            // 1. Make sure we join to the table name (otherwise we'll get incorrect entities).
            // 2. Provide our tracking status predicate
            var en = EntityName;
            IQueryable<TCustomFieldType> ourQuery = _trackingStatusRepo.GetQuery().Where(o => o.ForeignTableName == en).Where(customFieldPredicate);

            //Apply our foreign entity predicate
            IQueryable<TForeignEntity> foreignQuery = _foreignRepo.GetQuery().Where(foreignEntityPredicate);

            //If we have a join predicate, it means we need a left join executed (which means that we use a INNER join to return ALL the entities based on our predicate; whether they have a custom field or not).
            //If we don't have a join predicate, it means we don't care about entities without a custom field, so we use a LEFT join
            return joinPredicate != null
                ? MakeLeftJoin(ourQuery, foreignQuery).Where(joinPredicate)
                : MakeInnerJoin(ourQuery, foreignQuery);
        }

        /// <summary>
        /// This method is to generically generate a Queryable.Join() call on unknown types and properties.
        /// For example, we may want to join TCustomFieldType -> EmailDO. The keys used will be (.ForeignTableID -> .EmailID)
        /// If however, we want to join TCustomFieldType -> CustomerDO, the keys used will be (.ForeignTableID -> .CustomerID)
        /// Due to the fact that we don't use the same key name, we need to lookup the key using reflection.
        /// The below will generate code equivelant to (if joining to the EmailTable):
        /// emailRepo.GetQuery().GroupJoin
        /// (
        ///     customFieldRepo.GetQuery(),
        ///     (e) => e.EmailID,
        ///     (ts) => ts.ForeignTableID,
        ///     (e, ts) => new JoinResult { ForeignDO = e, TCustomFieldType = ts.FirstOrDefault() }
        /// );
        /// 
        /// Note that emailRepo.GetQuery() is any IQueryable, which means we _can_ have predicates, for example, querying emails sent by joesmith@gmail.com
        /// This also applies to customFieldQuery.
        /// The below method is just a helper, and users can provide predicates using the above methods
        /// </summary>
        protected IQueryable<JoinResult> MakeLeftJoin(IQueryable<TCustomFieldType> customFieldQuery, IQueryable<TForeignEntity> foreignQuery)
        {
            customFieldQuery = customFieldQuery.DefaultIfEmpty();

            //Grab our foreign key selector (in the form of (e) => e.[PrimaryKeyProperty]) - where PrimaryKeyProperty is the primary key of the entity
            Expression<Func<TForeignEntity, int>> foreignKeySelector = GetForeignKeySelectorExpression();

            //Make the join!
            return foreignQuery.GroupJoin
                (
                    customFieldQuery,
                    foreignKeySelector,
                    ts => ts == null ? -1 : ts.Id, // Null check is for our in-memory mocked queries
                    (foreignDO, customFieldDO) =>
                        new JoinResult
                        {
                            ForeignDO = foreignDO,
                            CustomFieldDO = customFieldDO.FirstOrDefault()
                        }
                );
        }

        /// <summary>
        /// This works as above, but for an INNER join
        /// </summary>
        protected IQueryable<JoinResult> MakeInnerJoin(IQueryable<TCustomFieldType> customFieldQuery, IQueryable<TForeignEntity> foreignQuery)
        {
            //Grab our foreign key selector (in the form of (e) => e.[PrimaryKeyProperty]) - where PrimaryKeyProperty is the primary key of the entity
            Expression<Func<TForeignEntity, int>> foreignKeySelector = GetForeignKeySelectorExpression();

            //Make the join!
            return foreignQuery.Join
                (
                    customFieldQuery,
                    foreignKeySelector,
                    ts => ts.Id,
                    (foreignDO, customFieldDO) =>
                        new JoinResult
                        {
                            ForeignDO = foreignDO,
                            CustomFieldDO = customFieldDO
                        }
                );
        }

        /// <summary>
        /// This method returns the primary key of the provided entity. Retrieves value based on the first property with the [Key] attribute.
        /// </summary>
        protected int GetKey(TForeignEntity entity)
        {
            int id = GetForeignKeySelectorExpression().Compile().Invoke(entity);
            if (id == 0)
                throw new Exception("Cannot be applied to new entities. Entities must be saved to the database before applying a custom field.");
            return id;
        }

        /// <summary>
        /// This method returns an expression which retrives the primary key of an entity. This is not executed immediately. When passed to Linq2SQL, it is translated into a SQL call (not in memory).
        /// </summary>
        protected Expression<Func<TForeignEntity, int>> GetForeignKeySelectorExpression()
        {
            Type foreignTableType = typeof(TForeignEntity);
            PropertyInfo foreignTableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression foreignProp = Expression.Parameter(foreignTableType);
            MemberExpression propertyAccessor = Expression.Property(foreignProp, foreignTableKey);
            Expression<Func<TForeignEntity, int>> foreignKeySelector = Expression.Lambda(propertyAccessor, new[] { foreignProp }) as Expression<Func<TForeignEntity, int>>;
            return foreignKeySelector;
        }

        private String _entityName;
        protected String EntityName
        {
            get
            {
                return _entityName ?? (_entityName = typeof (TForeignEntity).Name);
            }
        }

        private PropertyInfo _entityPrimaryKeyPropertyInfo;
        protected PropertyInfo EntityPrimaryKeyPropertyInfo
        {
            get
            {
                if (_entityPrimaryKeyPropertyInfo == null)
                {
                    List<PropertyInfo> keys = typeof(TForeignEntity).GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any()).ToList();
                    if (keys.Count > 1)
                    throw new Exception("Linked entity MUST have a single primary key. Composite keys are not supported.");
                    //If no primary key exists, we cannot use it
                    if (keys.Count == 0)
                    throw new Exception(
                        "Linked entity MUST have a single primary key. Entities without primary keys are not supported.");

                    _entityPrimaryKeyPropertyInfo = keys.First();
                }
                return _entityPrimaryKeyPropertyInfo;
            }
        }

        /// <summary>
        /// Class used to store the join result. We cannot use anonymous methods as typical in Linq2SQL, as we return the join, rather than immediately process it.
        /// </summary>
        protected class JoinResult
        {
            public TCustomFieldType CustomFieldDO;
            public TForeignEntity ForeignDO;
        }
    }
}
