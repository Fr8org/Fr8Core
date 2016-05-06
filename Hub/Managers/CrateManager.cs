using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Newtonsoft.Json;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Helpers;
using Fr8Data.Manifests;
using Fr8Data.States;
using StructureMap;

namespace Hub.Managers
{
    public partial class CrateManager : ICrateManager
    {

        public CrateDTO ToDto(Crate crate)
        {
            return crate != null ? CrateStorageSerializer.Default.ConvertToDto(crate) : null;
        }

        public Crate FromDto(CrateDTO crate)
        {
            return crate != null ? CrateStorageSerializer.Default.ConvertFromDto(crate) : null;
        }

        public ICrateStorage FromDto(CrateStorageDTO crateStorage)
        {
            return CrateStorageSerializer.Default.ConvertFromDto(crateStorage);
        }
        /// <summary>
        /// Use this method to edit CrateStorage repersented byt CrateStorageDTO property of some class instance. This method will return IDisposable updater.
        /// On Dispose it will write changes to the property specified by the Expression. 
        /// </summary>
        /// <param name="storageAccessExpression"></param>
        /// <returns></returns>
        public IUpdatableCrateStorage UpdateStorage(Expression<Func<CrateStorageDTO>> storageAccessExpression)
        {
            return new UpdatableCrateStorageStorage(storageAccessExpression);
        }

        /// <summary>
        /// Use this method to edit CrateStorage represented by string property of some class instance. This method will return IDisposable updater.
        /// On Dispose it will write changes to the property specified by the Expression. 
        /// </summary>
        /// <param name="storageAccessExpression"></param>
        /// <returns></returns>
        public IUpdatableCrateStorage UpdateStorage(Expression<Func<string>> storageAccessExpression)
        {
            return new UpdatableCrateStorageStorage(storageAccessExpression);
        }

        public string EmptyStorageAsStr()
        {
            return CrateStorageAsStr(new CrateStorage());
        }

        public string CrateStorageAsStr(ICrateStorage storage)
        {
            return JsonConvert.SerializeObject(CrateStorageSerializer.Default.ConvertToDto(storage));
        }

        public Crate CreateAuthenticationCrate(string label, AuthenticationMode mode, bool revocation)
        {
            return Crate.FromContent(label, new StandardAuthenticationCM()
            {
                Mode = mode,
                Revocation = revocation
            });
        }
        public Crate<FieldDescriptionsCM> CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields)
        {
            return Crate<FieldDescriptionsCM>.FromContent(label, new FieldDescriptionsCM() { Fields = fields.ToList() });
        }

        public T GetContentType<T>(string crate) where T : class
        {
            return this.GetStorage(crate)
                            .CrateContentsOfType<T>()
                            .FirstOrDefault();
        }
    }
}
