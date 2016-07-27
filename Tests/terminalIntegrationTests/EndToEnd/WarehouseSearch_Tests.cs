using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Data.Infrastructure;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class WarehouseSearch_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName => "terminalFr8Core";

        private QueryDTO QueryFixture()
        {
            return new QueryDTO()
            {
                Name = "Docusign Envelope",
                Criteria = new List<FilterConditionDTO>
                {
                    new FilterConditionDTO()
                    {
                        Field = "SentDate",
                        Operator = "gte",
                        Value = "21-04-2021"
                    }
                }
            };
        }

        private List<DocuSignEnvelopeCM> MtDataFixture_1()
        {
            return new List<DocuSignEnvelopeCM>()
            {
                new DocuSignEnvelopeCM()
                {
                    EnvelopeId = "FFE35085-C540-40AE-8C22-AC277348309B",
                    SentDate = new DateTime(2021, 04, 20)
                },
                new DocuSignEnvelopeCM()
                {
                    EnvelopeId = "0231C2B7-9A6B-4F71-8378-991BE375EBFA",
                    SentDate = new DateTime(2021, 04, 22)
                }
            };
        }

        [Test]
        public async Task WarehouseSearch_Query()
        {
            var mtData = MtDataFixture_1();

            CreateMtDataRecords(mtData);

            var url = GetHubApiBaseUrl() + "warehouses/query";
            var query = QueryFixture();

            var response = await HttpPostAsync<QueryDTO, JToken>(url, query);
            Assert.NotNull(response, "Response from warehouses/query is null.");

            var searchedData = response.ToObject<List<DocuSignEnvelopeCM>>();
            Assert.AreEqual(1, searchedData.Count, "Response from warehouses/query contains wrong number of results.");

            Assert.AreEqual(mtData[1].EnvelopeId, searchedData[0].EnvelopeId, "Response from warehouses/query contains wrong value for EnvelopeId.");
        }


        [Test]
        public async Task WarehouseAdd()
        {
            var url = GetHubApiBaseUrl() + "warehouses";
            var dataToAdd = new ManifestDescriptionCM()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test envelope added by WarehouseAdd test",
            };

            var crateStorage = new CrateStorage(Fr8.Infrastructure.Data.Crates.Crate.FromContent(null, dataToAdd));

            await HttpPostAsync<CrateStorageDTO, object>(url, CrateStorageSerializer.Default.ConvertToDto(crateStorage));

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository
                    .GetQuery()
                    .FirstOrDefault(x => x.UserName == TestUserEmail);

                Assert.NotNull(user, "Could not find test user in the database.");

                var addedEnvelope = uow.MultiTenantObjectRepository.Query<ManifestDescriptionCM>(user.Id, x => x.Id == dataToAdd.Id).FirstOrDefault();

                Assert.NotNull(addedEnvelope, "Failed to add new record to Warehouse using API");

                uow.MultiTenantObjectRepository.Delete<ManifestDescriptionCM>(user.Id, x => x.Id == dataToAdd.Id);

                Assert.AreEqual(dataToAdd.Name, addedEnvelope.Name, "Invalid value of Name property for stored data");
            }
        }

        private void CreateMtDataRecords(IEnumerable<DocuSignEnvelopeCM> data)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository
                    .GetQuery()
                    .FirstOrDefault(x => x.UserName == TestUserEmail);

                Assert.NotNull(user, "Could not find test user in the database.");

                foreach (var item in data)
                {
                    uow.MultiTenantObjectRepository.AddOrUpdate(user.Id, item);
                }

                uow.SaveChanges();
            }
        }
    }
}
