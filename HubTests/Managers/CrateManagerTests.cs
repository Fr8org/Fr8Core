using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.StructureMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;

namespace HubTests.Managers
{
    public class UnknownManifest
    {
    }

    [TestFixture]
    [Category("CrateManager")]
    public partial class CrateManagerTests
    {
        private ICrateManager _crateManager;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        //CreateErrorCrate is not used anywhere. Error crate should have MenifestType but currently it doesn't.

//        [Test]
//        public void CreateErrorCrate_ReturnsCrateDTO()
//        {
//            var crateManager = new CrateManager();
//            var errorMessage = "This is test error message";
//            var result = crateManager.CreateErrorCrate(errorMessage);
//            Assert.IsNotNull(result);
//            Assert.AreEqual("Retry Crate", result.Label);
//            Assert.AreEqual(errorMessage, result.Contents);
//
//
//        }

        [Test]
        public void FromNullCrateStorageDTO_ReturnsEmptyStorage()
        {
            Assert.AreEqual(_crateManager.FromDto((CrateStorageDTO)null).Count, 0);
        }
        
        [Test]
        public void IsEmptyStorageWithNull_ReturnsTrue()
        {
            Assert.IsTrue(_crateManager.IsEmptyStorage(null));
        }

        [Test]
        public void CrateWithNonRegisteredManifest_GetContentReturnsNull()
        {
            var crate = Crate.FromJson(new CrateManifestType("Unknown type", 66666666), "unknown value");

            Assert.IsNull(crate.Get());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CrateWithNonRegisteredManifest_FailsToCrateFromContent()
        {
            Crate.FromContentUnsafe("test", new UnknownManifest());
        }

        [Test]
        public void CrateWithNonRegisteredManifest_GetRaw()
        {
            const string manifest = "unknown value";
            var crate = Crate.FromJson(new CrateManifestType("Unknown type", 66666666), manifest);

            Assert.AreEqual(crate.GetRaw(), (JToken)manifest);
        }

        [Test]
        public void CrateWithRegistetedManifest_GetContent()
        {
            var crate = Crate.FromContent("test", TestManifest());

            crate.Get<StandardDesignTimeFieldsCM>();
        }

        [Test]
        public void CrateWithRegistetedManifestCorrect_IsOfType()
        {
            var crate = Crate.FromContent("test", TestManifest());

            Assert.IsTrue(crate.IsOfType<StandardDesignTimeFieldsCM>());
        }

        [Test]
        public void CrateWithRegistetedManifestIncorrect_IsOfType()
        {
            var crate = Crate.FromContent("test", TestManifest());

            Assert.IsFalse(crate.IsOfType<StandardConfigurationControlsCM>());
        }

        [Test]
        public void GetCrateStorage_KnownManifests()
        {
            var storageDto = GetKnownManifestsStorageDto();
            var storage = _crateManager.FromDto(storageDto);

            Assert.AreEqual(storageDto.Crates.Length, storage.Count);

            foreach (var crateDto in storageDto.Crates)
            {
                var dto = crateDto;
                Assert.NotNull(storage.FirstOrDefault(x => x.Label == dto.Label && 
                    x.Id == dto.Id && 
                    x.ManifestType.Type == dto.ManifestType && 
                    x.ManifestType.Id == dto.ManifestId &&
                    IsEquals(x.Get<StandardDesignTimeFieldsCM>(), dto.Contents.ToObject<StandardDesignTimeFieldsCM>())));
            }
        }

        [Test]
        public void GetCrateStorage_UnknownManifests()
        {
            var storageDto = GetUnknownManifestsStorageDto();
            var storage = _crateManager.FromDto(storageDto);

            Assert.AreEqual(storageDto.Crates.Length, storage.Count);

            foreach (var crateDto in storageDto.Crates)
            {
                var dto = crateDto;
                Assert.NotNull(storage.FirstOrDefault(x => x.Label == dto.Label &&
                    x.Id == dto.Id &&
                    x.ManifestType.Type == dto.ManifestType &&
                    x.ManifestType.Id == dto.ManifestId &&
                    Equals(x.GetRaw(), dto.Contents)));
            }
        }

        [Test]
        public void UpdateStorageStorageRewrite_Works()
        {
            var actionDto = new ActionDTO();
            
            actionDto.CrateStorage = GetKnownManifestsStorageDto();
            
            var newCrateStorageDto = GetKnownManifestsStorageDto("newValue");
            var newCrateStorage = _crateManager.FromDto(newCrateStorageDto);

            using (var updater = _crateManager.UpdateStorage(actionDto))
            {
                updater.CrateStorage.Clear();
                
                foreach (var crates in newCrateStorage)
                {
                    updater.CrateStorage.Add(crates);
                }
            }

            CheckStorageDTOs(newCrateStorageDto, actionDto.CrateStorage);
        }

        private static void CheckStorageDTOs(CrateStorageDTO a, CrateStorageDTO b)
        {
            foreach (var crateDto in a.Crates)
            {
                var dto = crateDto;
                Assert.NotNull(b.Crates.FirstOrDefault(x => x.Label == dto.Label &&
                    x.Id == dto.Id &&
                    x.ManifestType == dto.ManifestType &&
                    x.ManifestId == dto.ManifestId &&
                    JToken.DeepEquals(x.Contents, dto.Contents)));
            }
        }

        private static bool IsEquals(StandardDesignTimeFieldsCM a, StandardDesignTimeFieldsCM b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a.Fields == null && b.Fields == null)
            {
                return true;
            }

            if (a.Fields == null || b.Fields == null)
            {
                return false;
            }

            if (a.Fields.Count != b.Fields.Count)
            {
                return false;
            }

            foreach (var fieldDto in a.Fields)
            {
                var field = fieldDto;
                
                if (!b.Fields.Any(x => x.Key == field.Key && x.Value == field.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
