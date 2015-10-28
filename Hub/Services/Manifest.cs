using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Data.Interfaces;
using Data.Interfaces.Manifests;
using System.Reflection;
using Hub.Interfaces;
using Newtonsoft.Json;
using StructureMap;

namespace Hub.Services
{
    public class Manifest : IManifest
    {
        private readonly ICrateManager _curCrate;
        private readonly Dictionary<int, string> _curManifestDictionary;

        public Manifest()
        {
            _curCrate = ObjectFactory.GetInstance<ICrateManager>();
            _curManifestDictionary = CrateManifests.MANIFEST_CLASS_MAPPING_DICTIONARY;
        }

        public CrateDTO GetById(int id)
        {
            CrateDTO crateDTO = null;
            string manifestAssemblyName = null;

            _curManifestDictionary.TryGetValue(id, out manifestAssemblyName);

            if (!String.IsNullOrWhiteSpace(manifestAssemblyName))
            {
                var curAssemblyName = "Data";
                string fullyQualifiedName = string.Format("{0}.Interfaces.Manifests.{1}", curAssemblyName, manifestAssemblyName);

                Assembly assembly = Assembly.Load(curAssemblyName);
                Type cuAssemblyType = assembly.GetType(fullyQualifiedName);

                if (cuAssemblyType == null)
                    throw new ArgumentException(manifestAssemblyName);

                MethodInfo curMethodName = cuAssemblyType.GetMethod("GetProperties");
                var curObject = Activator.CreateInstance(cuAssemblyType);

                Type curReturnType = curMethodName.ReturnType;

                if (curReturnType == typeof(List<FieldDTO>))
                {
                    List<FieldDTO> curFieldDTO = (List<FieldDTO>)curMethodName.Invoke(curObject, new Object[] { cuAssemblyType });
                    crateDTO = _curCrate.CreateDesignTimeFieldsCrate(manifestAssemblyName, curFieldDTO.ToArray());
                }
            }

            return crateDTO;
        }
    }
}
