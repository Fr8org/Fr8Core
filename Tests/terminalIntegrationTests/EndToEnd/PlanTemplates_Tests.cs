using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8.Testing.Integration;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Testing.Integration.Tools.Plans;
using Fr8.Testing.Unit.Fixtures;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class PlanTemplates_Tests : BaseHubIntegrationTest
    {
        private IntegrationTestTools _plansHelper;

        public override string TerminalName => "terminalFr8Core";

        public PlanTemplates_Tests()
        {
            _plansHelper = new IntegrationTestTools(this);
        }

        // here we create a plan from an existing template, which you can view here https://maginot.atlassian.net/wiki/display/DDW/PlanTemplate+Example
        // we run the plan and check if the subplan jump has happened
        // after that we create a template from a new plan and compare it to an old one 

        [Test, Category("Integration.terminalFr8Core"), Ignore]
        public async Task Plan_Template_EndToEndTest()
        {
            string template = FixtureData.PlanTemplate();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUser = uow.UserRepository.GetQuery().Where(a => a.UserName == TestUserEmail).FirstOrDefault();
                var templateDTO = JsonConvert.DeserializeObject<PlanTemplateDTO>(template);
                var templateDO = Mapper.Map<PlanTemplateDO>(templateDTO);

                //set all ids to null, in order for EF to create a new template
                templateDO.Id = 0;
                templateDO.PlanNodeDescriptions.ForEach(a => { a.ParentNode = templateDO.PlanNodeDescriptions.FirstOrDefault(b => b.Id == a.ParentNodeId); });
                templateDO.PlanNodeDescriptions.ForEach(a => { a.Id = 0; a.ParentNodeId = null; });

                //update ids for ActivityTemplate reference
                foreach (var nodeDescription in templateDO.PlanNodeDescriptions)
                {
                    var activityCategoryParam = (int)ActivityCategory.Processors;
                    var activityTemplates = await HttpGetAsync<List<WebServiceActivitySetDTO>>(GetHubApiBaseUrl() + "webservices?id=" + activityCategoryParam);
                    var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name.ToLower() == nodeDescription.Name.Replace(" ", "").ToLower());
                    nodeDescription.ActivityDescription.ActivityTemplateId = apmActivityTemplate.Id;
                    Assert.NotNull(nodeDescription.ActivityDescription.ActivityTemplateId);
                }

                //adding template to the database
                templateDO.Fr8AccountId = currentUser.Id;
                uow.PlanTemplateRepository.Add(templateDO);
                uow.SaveChanges();

                //create a plan from template
                var planIdFromTemplate = await HttpPostAsync<string>(GetHubApiBaseUrl() + string.Format("plantemplates/create?id={0}&userId={1}", templateDO.Id, currentUser.Id), null);
                //run the plan
                var container = await _plansHelper.RunPlan(Guid.Parse(planIdFromTemplate));
                //get the payload
                var containerPayload = await HttpGetAsync<PayloadDTO>(GetHubApiBaseUrl() + string.Format("containers/payload?id={0}", container.Id));

                var payloads = containerPayload.CrateStorage.Crates.Where(a => a.Label == "ManuallyAddedPayload").ToList();
                var subplan_generated_payload = payloads.Where(a => Crate.FromDto(a).Get<StandardPayloadDataCM>().GetValueOrDefault("test1") == "test2").FirstOrDefault();
                Assert.NotNull(subplan_generated_payload, "It seems that that jump to subplan didn't happen");

                var new_template = await HttpPostAsync<PlanTemplateDTO>(GetHubApiBaseUrl() + string.Format("plantemplates?planId={0}&userId={1}", planIdFromTemplate, currentUser.Id), null);
                var new_templateDO = Mapper.Map<PlanTemplateDO>(new_template);

                Assert.NotNull(new_templateDO.StartingPlanNodeDescription);
                Assert.AreEqual(templateDO.PlanNodeDescriptions.Count, new_templateDO.PlanNodeDescriptions.Count);
                Assert.AreEqual(templateDO.PlanNodeDescriptions.SelectMany(a => a.Transitions)
                    .Count(), new_templateDO.PlanNodeDescriptions.SelectMany(a => a.Transitions).Count());
            }
        }
    }
}
