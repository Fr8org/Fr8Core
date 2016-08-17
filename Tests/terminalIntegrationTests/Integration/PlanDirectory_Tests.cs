using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Testing.Integration;

namespace terminalIntegrationTests.Integration
{
    [Explicit]
    public class PlanDirectory_Tests : BasePlanDirectoryIntegrationTest
    {
        private static PublishPlanTemplateDTO PlanTemplateDTO_1()
        {
            return new PublishPlanTemplateDTO()
            {
                Name = "Test PlanTemplate Name 1",
                Description = "Test PlanTemplate Description 1",
                ParentPlanId = Guid.NewGuid(),
                PlanContents = new PlanDTO
                        {
                            Id = Guid.Parse("b2d428b3-f016-4a11-999a-5a16c34f1dc3"),
                            Name = "Test PlanTemplate Name 1",
                        }
            };
        }

        [Test]
        public async Task PlanDirectory_PlanTemplateApi_Create_Update_Extract()
        {
            var planTemplateDTO = PlanTemplateDTO_1();
            await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plan_templates/", planTemplateDTO);

            try
            {
                var returnedPlanTemplateDTO = await HttpGetAsync<PublishPlanTemplateDTO>(
                    _baseUrl + "plan_templates/?id=" + planTemplateDTO.ParentPlanId.ToString()
                );

                Assert.NotNull(returnedPlanTemplateDTO);
                Assert.AreEqual(planTemplateDTO.Name, returnedPlanTemplateDTO.Name, "Returned PlanTemplateDTO does not match original PlanTemplateDTO (Name)");
                Assert.AreEqual(planTemplateDTO.Description, returnedPlanTemplateDTO.Description, "Returned PlanTemplateDTO does not match original PlanTemplateDTO (Description)");
                Assert.AreEqual(planTemplateDTO.ParentPlanId, returnedPlanTemplateDTO.ParentPlanId, "Returned PlanTemplateDTO does not match original PlanTemplateDTO (ParentPlanId)");
                Assert.AreEqual(JsonConvert.SerializeObject(planTemplateDTO.PlanContents), JsonConvert.SerializeObject(returnedPlanTemplateDTO.PlanContents), "Returned PlanTemplateDTO does not match original PlanTemplateDTO (ParentPlanId)");

                planTemplateDTO.Name = "Test PlanTemplate Name 1 (Updated)";
                planTemplateDTO.Description = "Test PlanTemplate Description 1 (Updated)";
                planTemplateDTO.PlanContents = new PlanDTO();

                await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plan_templates/", planTemplateDTO);

                returnedPlanTemplateDTO = await HttpGetAsync<PublishPlanTemplateDTO>(
                    _baseUrl + "plan_templates/?id=" + planTemplateDTO.ParentPlanId.ToString()
                );

                Assert.NotNull(returnedPlanTemplateDTO);
                Assert.AreEqual(planTemplateDTO.Name, returnedPlanTemplateDTO.Name, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (Name)");
                Assert.AreEqual(planTemplateDTO.Description, returnedPlanTemplateDTO.Description, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (Description)");
                Assert.AreEqual(planTemplateDTO.ParentPlanId, returnedPlanTemplateDTO.ParentPlanId, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (ParentPlanId)");
                Assert.AreEqual(JsonConvert.SerializeObject(planTemplateDTO.PlanContents), JsonConvert.SerializeObject(returnedPlanTemplateDTO.PlanContents), "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (ParentPlanId)");
            }
            finally
            {
                await HttpDeleteAsync(_baseUrl + "plan_templates/?id=" + planTemplateDTO.ParentPlanId.ToString());
                
            }
        }

        [Test]
        public async Task PlanDirectory_CreatePlan()
        {
            var planTemplateDTO = PlanTemplateDTO_1();
            var result = await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plan_templates/", planTemplateDTO);

            try
            {

                var createPlanResult = await HttpPostAsync<JToken>(
                    _baseUrl + "plan_templates/createplan?id=" + planTemplateDTO.ParentPlanId.ToString(), null);

                Assert.NotNull(createPlanResult, "Failed to create plan.");

                var redirectUrl = createPlanResult["redirectUrl"].Value<string>();
                Assert.IsNotEmpty(redirectUrl, "CreatePlan response does not contain RedirectUrl property.");

                var planId = ExtractPlanIdFromRedirectUrl(redirectUrl);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var plan = uow.PlanRepository.GetById<PlanDO>(planId);
                    Assert.NotNull(plan, "Created plan was not found");
                    Assert.AreEqual(planTemplateDTO.Name, plan.Name, "Created plan was not found");

                    // TODO: another user?
                    //var user2 = uow.UserRepository.GetQuery()
                    //    .FirstOrDefault(x => x.UserName == "IntegrationTestUser1");
                    //Assert.NotNull(user2, "IntegrationTestUser1 was not found.");
                    //Assert.AreEqual(user2.Id, plan.Fr8AccountId, "Created plan does not belong to IntegrationTestUser1");
                }
            }
            finally
            {
                await AuthenticateWebApi(TestUserEmail, TestUserPassword);
                await HttpDeleteAsync(_baseUrl + "plan_templates/?id=" + planTemplateDTO.ParentPlanId.ToString());
            }
        }

        private static Guid ExtractPlanIdFromRedirectUrl(string redirectUrl)
        {
            var urlTokens = redirectUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return Guid.Parse(urlTokens[4]);
        }
    }
}
