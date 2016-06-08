using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Manifests;
using HealthMonitor.Utility;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class WarehouseSearch_Tests : BaseHubIntegrationTest
    {
        private const string TestRoleName = "TestRole";
        private const string OtherUserName = "IntegrationTestUser1";

        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        private QueryDTO QueryFixture()
        {
            return new QueryDTO()
            {
                Name = "Docusign Envelope",
                Criteria = new List<FilterConditionDTO>()
                {
                    new FilterConditionDTO()
                    {
                        Field = "SentDate",
                        Operator = "gte",
                        Value = "22-04-2021"
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

            var url = GetHubApiBaseUrl() + "warehouse/query";
            var query = QueryFixture();

            var response = await HttpPostAsync<QueryDTO, JToken>(url, query);
            Assert.NotNull(response, "Response from warehouse/query is null.");

            var searchedData = response.ToObject<List<DocuSignEnvelopeCM>>();
            Assert.AreEqual(1, searchedData.Count, "Response from warehouse/query contains wrong number of results.");

            Assert.AreEqual(mtData[1].EnvelopeId, searchedData[0].EnvelopeId, "Response from warehouse/query contains wrong value for EnvelopeId.");
        }

        private void CreateMtDataRecords(IEnumerable<DocuSignEnvelopeCM> data)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository
                    .GetQuery()
                    .Where(x => x.UserName == TestUserEmail)
                    .FirstOrDefault();

                Assert.NotNull(user, "Could not find test user in the database.");

                foreach (var item in data)
                {
                    uow.MultiTenantObjectRepository.AddOrUpdate(user.Id, item);
                }

                uow.SaveChanges();
            }
        }

        private void DeleteMtDataRecords(IEnumerable<DocuSignEnvelopeCM> data)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository
                    .GetQuery()
                    .Where(x => x.UserName == TestUserEmail)
                    .FirstOrDefault();

                Assert.NotNull(user, "Could not find test user in the database.");

                foreach (var item in data)
                {
                    uow.MultiTenantObjectRepository.Delete<DocuSignEnvelopeCM>(user.Id, x => x.EnvelopeId == item.EnvelopeId);
                }

                uow.SaveChanges();
            }
        }

        private void CreateRoles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var role = uow.AspNetRolesRepository.GetQuery()
                    .FirstOrDefault(x => x.Name == TestRoleName);
                if (role == null)
                {
                    role = new AspNetRolesDO()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = TestRoleName
                    };

                    uow.AspNetRolesRepository.Add(role);
                }

                var testUser = uow.UserRepository.GetQuery()
                    .FirstOrDefault(x => x.UserName == TestUserEmail);
                Assert.NotNull(testUser, $"Test user {TestUserEmail} is not found");

                var otherTestUser = uow.UserRepository.GetQuery()
                    .FirstOrDefault(x => x.UserName == OtherUserName);
                Assert.NotNull(otherTestUser, $"Other test user {OtherUserName} is not found");

                var testUserRole = new AspNetUserRolesDO()
                {
                    RoleId = role.Id,
                    UserId = testUser.Id
                };
                uow.AspNetUserRolesRepository.Add(testUserRole);

                var otherTestUserRole = new AspNetUserRolesDO()
                {
                    RoleId = role.Id,
                    UserId = otherTestUser.Id
                };
                uow.AspNetUserRolesRepository.Add(otherTestUserRole);

                uow.SaveChanges();
            }
        }

        private void DeleteRoles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var role = uow.AspNetRolesRepository.GetQuery()
                    .FirstOrDefault(x => x.Name == TestRoleName);
                Assert.NotNull(role, $"Test role {TestRoleName} is not found");

                var testUser = uow.UserRepository.GetQuery()
                    .FirstOrDefault(x => x.UserName == TestUserEmail);
                Assert.NotNull(testUser, $"Test user {TestUserEmail} is not found");

                var otherTestUser = uow.UserRepository.GetQuery()
                    .FirstOrDefault(x => x.UserName == OtherUserName);
                Assert.NotNull(otherTestUser, $"Other test user {OtherUserName} is not found");

                var testUserRole = uow.AspNetUserRolesRepository.GetQuery()
                    .FirstOrDefault(x => x.RoleId == role.Id && x.UserId == testUser.Id);
                if (testUserRole != null)
                {
                    uow.AspNetUserRolesRepository.Remove(testUserRole);
                }

                var otherTestUserRole = uow.AspNetUserRolesRepository.GetQuery()
                    .FirstOrDefault(x => x.RoleId == role.Id && x.UserId == otherTestUser.Id);
                if (otherTestUserRole != null)
                {
                    uow.AspNetUserRolesRepository.Remove(otherTestUserRole);
                }

                uow.SaveChanges();
            }
        }
    }
}
