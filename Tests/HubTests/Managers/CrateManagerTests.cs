using System;
using System.Linq;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Hub.StructureMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting.Fixtures;
using UtilitiesTesting;

namespace HubTests.Managers
{
    public class UnknownManifest
    {
    }

    [TestFixture]
    [Category("CrateManager")]
    public partial class CrateManagerTests : BaseTest
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
        public void CanAddLogMessageToContainerDO()
        {
            // Arrange
            var curContainerDO = FixtureData.TestContainer1();
            var curLogItemList = FixtureData.LogItemDTOList();
            var curLabel = "Crate Manager Can Add Log Message To ContainerDO Test";
            var curCrateDTOContents = "{\"Item\":[{\"Name\":\"LogItemDTO1\",\"PrimaryCategory\":\"Container\",\"SecondaryCategory\":\"LogItemDTO Generator\",\"Activity\":\"Add Log Message\",\"Data\":\"\"}],\"ManifestType\":13,\"ManifestId\":13,\"ManifestName\":\"Standard Logging Crate\"}";

            //Act
            _crateManager.AddLogMessage(curLabel, curLogItemList, curContainerDO);
            var updatedCrate = _crateManager.GetStorage(curContainerDO.CrateStorage).FirstCrateOrDefault<StandardLoggingCM>(x => x.Label == curLabel);

            //Assert
            Assert.IsNotNull(updatedCrate);

            Assert.AreEqual(updatedCrate.Content.Item.Count, curLogItemList.Count);
            int eq = 0;

            foreach (var item in updatedCrate.Content.Item)
            {
                foreach (var refItem in curLogItemList)
                {
                    if (item.Activity == refItem.Activity &&
                        item.Data == refItem.Data &&
                        item.Name == refItem.Name &&
                        item.PrimaryCategory == refItem.PrimaryCategory &&
                        item.SecondaryCategory == refItem.SecondaryCategory)
                    {
                        eq++;
                        break;
                    }
                }
            }

            Assert.AreEqual(eq, curLogItemList.Count);
        }

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
        public void MergeContentFields_ReturnsStandardDesignTimeFieldsMS()
        {
            var result = _crateManager.MergeContentFields(FixtureData.TestCrateDTO1());
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Fields.Count);
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

            crate.Get<FieldDescriptionsCM>();
        }

        [Test]
        public void CrateWithRegistetedManifestCorrect_IsOfType()
        {
            var crate = Crate.FromContent("test", TestManifest());

            Assert.IsTrue(crate.IsOfType<FieldDescriptionsCM>());
        }

        [Test]
        public void CrateWithRegistetedManifestIncorrect_IsOfType()
        {
            var crate = Crate.FromContent("test", TestManifest());

            Assert.IsFalse(crate.IsOfType<StandardConfigurationControlsCM>());
        }

        [Test]
        public void GetCrateStorageFromDto_KnownManifests()
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
                    IsEquals(x.Get<FieldDescriptionsCM>(), dto.Contents.ToObject<FieldDescriptionsCM>())));
            }
        }

        [Test]
        public void GetCrateStorageFromDto_UnknownManifests()
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
        public void GetCrateFromDto_UnknownManifest()
        {
            var crateDto = TestUnknownCrateDto("id1", "value");
            var crate = _crateManager.FromDto(crateDto);

            Assert.AreEqual(crate.Id, crateDto.Id);
            Assert.AreEqual(crate.Label, crateDto.Label);
            Assert.AreEqual(crate.ManifestType.Type, crateDto.ManifestType);
            Assert.AreEqual(crate.ManifestType.Id, crateDto.ManifestId);
            Assert.AreEqual(crate.GetRaw(), (JToken)"value");
        }

        [Test]
        public void GetCrateFromDto_KnownManifest()
        {
            var crateDto = TestKnownCrateDto("id1", "value");
            var crate = _crateManager.FromDto(crateDto);

            Assert.AreEqual(crate.Id, crateDto.Id);
            Assert.AreEqual(crate.Label, crateDto.Label);
            Assert.AreEqual(crate.ManifestType.Type, crateDto.ManifestType);
            Assert.AreEqual(crate.ManifestType.Id, crateDto.ManifestId);
            Assert.AreEqual(crate.Get<FieldDescriptionsCM>().Fields[0].Key, "key");
            Assert.AreEqual(crate.Get<FieldDescriptionsCM>().Fields[0].Value, "value");
        }

        [Test]
        public void UpdateStorageDtoRewrite_Works()
        {
            var activityDto = new ActivityDTO();

            activityDto.CrateStorage = GetKnownManifestsStorageDto();

            var newCrateStorageDto = GetKnownManifestsStorageDto("newValue");
            var newCrateStorage = _crateManager.FromDto(newCrateStorageDto);

            using (var crateStorage = _crateManager.GetUpdatableStorage(activityDto))
            {
                crateStorage.Clear();

                foreach (var crates in newCrateStorage)
                {
                    crateStorage.Add(crates);
                }
            }

            CheckStorageDTOs(newCrateStorageDto, activityDto.CrateStorage);
        }

        [Test]
        public void TransformStandardTableDataToStandardPayloadData_ShouldProcessFirstRowAndHaveCorrectKeys()
        {
            var payload = _crateManager.TransformStandardTableDataToStandardPayloadData("Some type", GetTestTable());
            Assert.AreEqual("SerbianWord", payload.PayloadObjects[0].PayloadObject[0].Key);
            Assert.AreEqual("Pouzdan", payload.PayloadObjects[0].PayloadObject[0].Value);
            Assert.AreEqual("EnglishWord", payload.PayloadObjects[0].PayloadObject[1].Key);
            Assert.AreEqual("Reliable", payload.PayloadObjects[0].PayloadObject[1].Value);
            Assert.AreEqual(2, payload.PayloadObjects.Count());
        }

        [Test]
        public void TransformStandardTableDataToStandardPayloadData_ShouldNotProcessFirstRow()
        {
            var payload = _crateManager.TransformStandardTableDataToStandardPayloadData("Some type", GetTestTableWithHeaders());
            Assert.AreEqual("SerbianWord", payload.PayloadObjects[0].PayloadObject[0].Key);
            Assert.AreEqual("Pouzdan", payload.PayloadObjects[0].PayloadObject[0].Value);
            Assert.AreEqual("EnglishWord", payload.PayloadObjects[0].PayloadObject[1].Key);
            Assert.AreEqual("Reliable", payload.PayloadObjects[0].PayloadObject[1].Value);
            Assert.AreEqual(2, payload.PayloadObjects.Count());
        }

        [Test]
        public void UpdateStorageStringRewrite_Works()
        {
            var actionDo = new ActivityDO();

            actionDo.CrateStorage = JsonConvert.SerializeObject(GetKnownManifestsStorageDto());

            var newCrateStorageDto = GetKnownManifestsStorageDto("newValue");
            var newCrateStorage = _crateManager.FromDto(newCrateStorageDto);

            using (var crateStorage = _crateManager.GetUpdatableStorage(actionDo))
            {
                crateStorage.Clear();

                foreach (var crates in newCrateStorage)
                {
                    crateStorage.Add(crates);
                }
            }

            CheckStorageDTOs(newCrateStorageDto, JsonConvert.DeserializeObject<CrateStorageDTO>(actionDo.CrateStorage));
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

        private static bool IsEquals(FieldDescriptionsCM a, FieldDescriptionsCM b)
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
