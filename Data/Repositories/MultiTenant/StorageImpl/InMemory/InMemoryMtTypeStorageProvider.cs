using System;
using System.Collections.Generic;
using System.Linq;
using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant.InMemory
{
    class InMemoryMtTypeStorageProvider : IMtTypeStorageProvider
    {
        private readonly IMtTypeStorage _storage;
        private static readonly List<MtTypeDefinition> Types = new List<MtTypeDefinition>();
        private readonly List<MtTypeDefinition> _newTypes = new List<MtTypeDefinition>();

        public InMemoryMtTypeStorageProvider(IMtTypeStorage storage)
        {
            _storage = storage;
        }

        public IEnumerable<MtTypeDefinition> ListTypes(ISqlConnectionProvider connectionProvider)
        {
            lock (Types)
            {
                return Types.ToArray();
            }
        }

        public IEnumerable<MtTypeReference> ListTypeReferences(ISqlConnectionProvider connectionProvider)
        {
            lock (Types)
            {
                return Types.Select(x => new MtTypeReference(x.Alias, x.ClrType, x.Id)).ToArray();
            }
        }

        public IEnumerable<MtTypePropertyReference> ListTypePropertyReferences(ISqlConnectionProvider connectionProvider, Guid typeId)
        {
            lock (Types)
            {
                var type = Types.FirstOrDefault(x => x.Id == typeId);

                if (type == null || type.Properties == null)
                {
                    return new MtTypePropertyReference[0];
                }

                return type.Properties.Select(x => new MtTypePropertyReference(x.DeclaringType.Id, x.MtPropertyType.ClrType, x.MtPropertyType.Id, x.Name, x.Index)).ToArray();
            }
        }

        public MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, Type type)
        {
            MtTypeDefinition typeDef;

            if (!TryLoadType(connectionProvider, type, out typeDef))
            {
                return null;
            }

            return new MtTypeReference(typeDef.Alias, typeDef.ClrType, typeDef.Id);
        }

        public MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, Guid typeId)
        {
            MtTypeDefinition typeDef;

            lock (Types)
            {
                typeDef = Types.FirstOrDefault(x => x.Id == typeId);
                if (typeDef == null)
                {
                    return null;
                }

                return new MtTypeReference(typeDef.Alias, typeDef.ClrType, typeDef.Id);
            }
        }

        public MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, string alias)
        {
            MtTypeDefinition typeDef;

            lock (Types)
            {
                typeDef = Types.FirstOrDefault(x => x.Alias == alias);
                if (typeDef == null)
                {
                    return null;
                }

                return new MtTypeReference(typeDef.Alias, typeDef.ClrType, typeDef.Id);
            }
        }

        public bool TryLoadType(ISqlConnectionProvider connectionProvider, Type clrType, out MtTypeDefinition mtType)
        {
            lock (Types)
            {
                mtType = Types.FirstOrDefault(x => x.ClrType == clrType);
                return mtType != null;
            }
        }

        public void PersistType(ISqlConnectionProvider connectionProvider, MtTypeDefinition mtType)
        {
            _newTypes.Add(mtType);
        }

        public void SaveChanges(ISqlConnectionProvider connectionProvider)
        {
            using (var typeLock = _storage.AccureTypeTransactionLock())
            {
                lock (Types)
                {
                    foreach (var mtTypeDefinition in _newTypes)
                    {
                        if (!typeLock.IsTypePersisted(mtTypeDefinition.Id))
                        {
                            Types.Add(mtTypeDefinition);
                        }
                    }

                    typeLock.Commit(_newTypes.Select(x => x.Id));
                    _newTypes.Clear();
                }
            }
        }
    }
}