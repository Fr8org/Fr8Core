using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.PlanTemplates;
using Data.Interfaces.Manifests;
using Data.States;
using HealthMonitor.Utility;
using Hub.Interfaces;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminaBaselTests.Tools.Activities;
using terminaBaselTests.Tools.Plans;
using UtilitiesTesting.Fixtures;

namespace terminalIntegrationTests.EndToEnd
{
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

        [Test, Category("Integration.terminalFr8Core")]
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
                    var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Processors };
                    var activityTemplates = await HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(GetHubApiBaseUrl() + "webservices/activities", activityCategoryParam);
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

                //clean up after test
                var templatesForIntegrationUser = uow.PlanTemplateRepository.GetQuery().Where(a => a.User.Id == currentUser.Id).ToList();
                templatesForIntegrationUser.ForEach(a => uow.PlanTemplateRepository.Remove(a));
                uow.SaveChanges();
            }
        }
    }
}
