using System;
using System.Collections.Generic;
using System.Reflection;
using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant
{
    partial class MtTypeStorage : IMtTypeStorage
    {
        private readonly IMtObjectConverter _typeConverter;
        private readonly Dictionary<Type, MtTypeDefinition> _clrTypeMappings = new Dictionary<Type, MtTypeDefinition>();
        private readonly HashSet<Guid> _isUnsavedType = new HashSet<Guid>();

        public MtTypeStorage(IMtObjectConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }

        public MtTypeDefinition ResolveType(ISqlConnectionProvider connectionProvider, Type clrType, IMtTypeStorageProvider typeStorageProvider, bool storeIfNew)
        {
            return ResolveMtType(connectionProvider, clrType, typeStorageProvider, false, storeIfNew);
        }

        private void CacheTypeDependences(ISqlConnectionProvider connectionProvider, MtTypeDefinition typeDefinition, IMtTypeStorageProvider typeStorageProvider)
        {
            if (_clrTypeMappings.ContainsKey(typeDefinition.ClrType))
            {
                return;
            }

            // this type was not saved to the DB yet. Request storage provider to persist this type in storage provider's unit of work scope.
            // this logic shall not create duplicates because storage provider checks if type was alredy persisted before saving.

            if (_isUnsavedType.Contains(typeDefinition.Id))
            {
                typeStorageProvider.PersistType(connectionProvider, typeDefinition);
            }

            _clrTypeMappings[typeDefinition.ClrType] = typeDefinition;

            if (typeDefinition.Properties != null)
            {
                foreach (var mtPropertyInfo in typeDefinition.Properties)
                {
                    CacheTypeDependences(connectionProvider, mtPropertyInfo.MtPropertyType, typeStorageProvider);
                }
            }
        }

        private MtTypeDefinition ResolveMtType(ISqlConnectionProvider connectionProvider, Type clrType, IMtTypeStorageProvider typeStorageProvider, bool forceComplexType, bool storeIfNew)
        {
            MtTypeDefinition typeDefinition;

            lock (_clrTypeMappings)
            {
                if (!_clrTypeMappings.TryGetValue(clrType, out typeDefinition))
                {
                    if (typeStorageProvider.TryLoadType(connectionProvider, clrType, out typeDefinition))
                    {
                        CacheTypeDependences(connectionProvider, typeDefinition, typeStorageProvider);
                        return typeDefinition;
                    }

                    if (!storeIfNew)
                    {
                        return null;
                    }

                    typeDefinition = BuildMtType(connectionProvider, clrType, typeStorageProvider, forceComplexType);
                    typeStorageProvider.PersistType(connectionProvider, typeDefinition);
                    _clrTypeMappings[clrType] = typeDefinition;
                    _isUnsavedType.Add(typeDefinition.Id);
                }
            }

            return typeDefinition;
        }

        private void ConfirmTypePersistence(Guid mtTypeId)
        {
            _isUnsavedType.Remove(mtTypeId);
        }

        public ITypeTransactionLock AccureTypeTransactionLock()
        {
            return new TypeSaveLock(_clrTypeMappings, this);
        }
        
        private bool CheckTypePersisted(Guid typeId)
        {
            return !_isUnsavedType.Contains(typeId);
        }
        
        private MtTypeDefinition BuildMtType(ISqlConnectionProvider connectionProvider, Type clrType, IMtTypeStorageProvider typeStorageProvider, bool forceComplexType)
        {
            if (_typeConverter.IsPrimitiveType(clrType))
            {
                return MtTypeDefinition.MakePrimitive(Guid.NewGuid(), clrType);
            }

            // forceComplexType is the hack that prevents MT type system from traversing types of complex properties. 
            // Instead we mark such properties as 'complex' at later will store them as JSON
            if (forceComplexType)
            {
                return MtTypeDefinition.MakeComplexType(Guid.NewGuid(), clrType);
            }

            var mtType = MtTypeDefinition.MakeType(Guid.NewGuid(), clrType);
            int propId = 0;

            foreach (var prop in clrType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                // prevent type parsing for properties: set forceComplexType = true
                var propType = ResolveMtType(connectionProvider, prop.PropertyType, typeStorageProvider, true, true);
                mtType.Properties.Add(new MtPropertyInfo(propId++, prop.Name, mtType, propType));
            }

            return mtType;
        }
    }
}