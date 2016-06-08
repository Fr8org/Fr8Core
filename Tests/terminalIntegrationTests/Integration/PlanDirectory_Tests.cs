using System;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using HealthMonitor.Utility;

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
                PlanContents = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(new { activities = new string[] { } }))
            };
        }

        [Test]
        public async Task PlanDirectory_PlanTemplateApi_Create_Update_Extract()
        {
            var planTemplateDTO = PlanTemplateDTO_1();
            await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plantemplates/", planTemplateDTO);

            var returnedPlanTemplateDTO = await HttpGetAsync<PublishPlanTemplateDTO>(
                _baseUrl + "plantemplates/?id=" + planTemplateDTO.ParentPlanId.ToString()
            );

            Assert.NotNull(returnedPlanTemplateDTO);
            Assert.AreEqual(planTemplateDTO.Name, returnedPlanTemplateDTO.Name, "Returned PlanTemplateDTO does not match original PlanTemplateDTO (Name)");
            Assert.AreEqual(planTemplateDTO.Description, returnedPlanTemplateDTO.Description, "Returned PlanTemplateDTO does not match original PlanTemplateDTO (Description)");
            Assert.AreEqual(planTemplateDTO.ParentPlanId, returnedPlanTemplateDTO.ParentPlanId, "Returned PlanTemplateDTO does not match original PlanTemplateDTO (ParentPlanId)");
            Assert.AreEqual(JsonConvert.SerializeObject(planTemplateDTO.PlanContents), JsonConvert.SerializeObject(returnedPlanTemplateDTO.PlanContents), "Returned PlanTemplateDTO does not match original PlanTemplateDTO (ParentPlanId)");

            planTemplateDTO.Name = "Test PlanTemplate Name 1 (Updated)";
            planTemplateDTO.Description = "Test PlanTemplate Description 1 (Updated)";
            planTemplateDTO.PlanContents = JsonConvert.DeserializeObject<JToken>(
                JsonConvert.SerializeObject(new { subplans = new string[] { } })
            );

            await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plantemplates/", planTemplateDTO);

            returnedPlanTemplateDTO = await HttpGetAsync<PublishPlanTemplateDTO>(
                _baseUrl + "plantemplates/?id=" + planTemplateDTO.ParentPlanId.ToString()
            );

            Assert.NotNull(returnedPlanTemplateDTO);
            Assert.AreEqual(planTemplateDTO.Name, returnedPlanTemplateDTO.Name, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (Name)");
            Assert.AreEqual(planTemplateDTO.Description, returnedPlanTemplateDTO.Description, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (Description)");
            Assert.AreEqual(planTemplateDTO.ParentPlanId, returnedPlanTemplateDTO.ParentPlanId, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (ParentPlanId)");
            Assert.AreEqual(JsonConvert.SerializeObject(planTemplateDTO.PlanContents), JsonConvert.SerializeObject(returnedPlanTemplateDTO.PlanContents), "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (ParentPlanId)");
        }
    }
}
