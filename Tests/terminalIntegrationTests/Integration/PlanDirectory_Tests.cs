using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Fr8.TerminalBase.Interfaces;
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
                PlanContents = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(new { activities = new string[] { } }))
            };
        }

        [Test]
        public async Task PlanDirectory_PlanTemplateApi_Create_Update_Extract()
        {
            var planTemplateDTO = PlanTemplateDTO_1();
            await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plan_templates/", planTemplateDTO);

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
            planTemplateDTO.PlanContents = JsonConvert.DeserializeObject<JToken>(
                JsonConvert.SerializeObject(new { subplans = new string[] { } })
            );

            await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plan_templates/", planTemplateDTO);

            returnedPlanTemplateDTO = await HttpGetAsync<PublishPlanTemplateDTO>(
                _baseUrl + "plan_templates/?id=" + planTemplateDTO.ParentPlanId.ToString()
            );

            Assert.NotNull(returnedPlanTemplateDTO);
            Assert.AreEqual(planTemplateDTO.Name, returnedPlanTemplateDTO.Name, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (Name)");
            Assert.AreEqual(planTemplateDTO.Description, returnedPlanTemplateDTO.Description, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (Description)");
            Assert.AreEqual(planTemplateDTO.ParentPlanId, returnedPlanTemplateDTO.ParentPlanId, "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (ParentPlanId)");
            Assert.AreEqual(JsonConvert.SerializeObject(planTemplateDTO.PlanContents), JsonConvert.SerializeObject(returnedPlanTemplateDTO.PlanContents), "Returned PlanTemplateDTO does not match updated PlanTemplateDTO (ParentPlanId)");

            await HttpDeleteAsync(_baseUrl + "plan_templates/?id=" + planTemplateDTO.ParentPlanId.ToString());
        }

        [Test]
        public async Task PlanDirectory_CreatePlan()
        {
            var createdPlanId = Guid.NewGuid();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            hubCommunicatorMock.Setup(x => x.LoadPlan(It.IsAny<JToken>()))
                .Returns(() => Task.FromResult<PlanEmptyDTO>(new PlanEmptyDTO() { Id = createdPlanId }));

            System.Diagnostics.Debug.WriteLine("HubCommunicator Mock created.");

            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);

            System.Diagnostics.Debug.WriteLine("HubCommunicator Mock injected.");

            var planTemplateDTO = PlanTemplateDTO_1();
            await HttpPostAsync<PublishPlanTemplateDTO, string>(_baseUrl + "plan_templates/", planTemplateDTO);

            System.Diagnostics.Debug.WriteLine("PlanTemplate created.");

            await AuthenticateWebApi("IntegrationTestUser1", "fr8#s@lt!");

            System.Diagnostics.Debug.WriteLine("Authenticated with IntegrationTestUser1.");

            var createPlanResult = await HttpPostAsync<JToken>(
                _baseUrl + "plan_templates/createplan?id=" + planTemplateDTO.ParentPlanId.ToString(), null);

            System.Diagnostics.Debug.WriteLine("CreatePlan succeded.");

            hubCommunicatorMock.Verify(x => x.LoadPlan(It.IsAny<JToken>()), Times.Once());
        
            Assert.NotNull(createPlanResult);
        
            var redirectUrl = createPlanResult["RedirectUrl"].Value<string>();
            Assert.IsNotEmpty(redirectUrl);
            Assert.True(redirectUrl.EndsWith("/dashboard/plans/" + createdPlanId.ToString() + "/builder?viewMode=plan"));
        }
    }
}
