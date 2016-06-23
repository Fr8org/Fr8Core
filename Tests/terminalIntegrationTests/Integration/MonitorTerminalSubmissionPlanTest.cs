using Atlassian.Jira;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using Fr8.Testing.Integration;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using terminalGoogle.Services;
using Newtonsoft.Json;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using Fr8.TerminalBase.Interfaces;
using System.Diagnostics;
using System.Configuration;
using AutoMapper;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.Infrastructure;
using System.Web;

namespace terminalIntegrationTests.Integration
{
    [Explicit]
    public class MonitorTerminalSubmissionPlanTest : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get
            {
                return "terminalGoogle";
            }
        }

        private readonly string jiraToken = @"{""Terminal"":null,""Username"":""fr8_atlassian_test@fr8.co"",""Password"":""shoggoth34"",""Domain"":""https://maginot.atlassian.net"",""IsDemoAccount"":false}";

        private readonly string slackToken = "xoxp-7518126694-28009203829-49624084150-131bd1c65b";

        [Test]
        public async Task MonitorTerminalSubmissionPlan()
        {
            AutoMapperBootstrapper.ConfigureAutoMapper();

            var guidTestId = Guid.NewGuid();

            var userId = AddTokens();

            var googleEventUrl = ConfigurationManager.AppSettings["GoogleFormEventWebServerUrl"];

            //Trigger creating Plan
            await RestfulServiceClient.PostAsync(new Uri(googleEventUrl), new { fr8_user_id = userId });

            //Reconfiguring plan activities 
            var url = $"{GetHubApiBaseUrl()}/plans?name=MonitorSubmissionTerminalForm&visibility=2";
            var response = await RestfulServiceClient.GetAsync(new Uri(url), null, await GetHMACHeader(new Uri(url), userId));

            var plans = JsonConvert.DeserializeObject<IEnumerable<PlanDTO>>(response);
            var plan = plans.FirstOrDefault().Plan.SubPlans.FirstOrDefault();

            if(plan.Activities.Where(a => a.Ordering == 8).FirstOrDefault() != null)
            {
                var deleteActivityUrl = GetHubApiBaseUrl() + "activities/delete/" + plan.Activities.Where(a => a.Ordering == 8).FirstOrDefault().Id;
                await RestfulServiceClient.DeleteAsync(new Uri(deleteActivityUrl), null, await GetHMACHeader(new Uri(deleteActivityUrl), userId));
            }

            await ConfigureJira(plan.Activities.Where(a => a.Ordering == 5).FirstOrDefault().Id, userId);
            await ConfigureMessage(plan.Activities.Where(a => a.Ordering == 6).FirstOrDefault().Id, userId, guidTestId.ToString());
            await ConfigureSlack(plan.Activities.Where(a => a.Ordering == 7).FirstOrDefault().Id, userId);

            //Run plan again after reconfigure
            var runUrl = GetHubApiBaseUrl() + "plans/run?planId=" + plans.FirstOrDefault().Plan.Id;
            await RestfulServiceClient.PostAsync(new Uri(runUrl), new List<CrateDTO>(), null, await GetHMACHeader(new Uri(runUrl), userId));

            await SubmitForm(googleEventUrl, guidTestId.ToString());

            //Waiting for Plan execution
            await Task.Delay(40000);

            //Searching for created jira issue
            Jira jira = CreateRestClient(jiraToken);
            var issues = jira.GetIssuesFromJql("summary ~ " + guidTestId.ToString());

            //Searching for slack message
            var slackUrl = "https://slack.com/api/search.messages?token="+ slackToken + "&query=" + guidTestId.ToString() + "%20in%3A%23general";
            var result = await RestfulServiceClient.GetAsync(new Uri(slackUrl));

            var slackSearchResult = JObject.Parse(result);
            var total = (int)slackSearchResult.SelectToken("messages.pagination.total_count");

            Assert.IsTrue(issues.Count() > 0,"Couldn't find jira issue");
            
            Assert.IsTrue(total != 0,"Couldn't find slack message");
        }

        private async Task SubmitForm(string url, string guid)
        {
            var client = new HttpClient();
            //emulates submiting form
            var formData = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("user_id","fr8test1@gmail.com"),
                    new KeyValuePair<string, string>("response","Github Pull Request URL=aaa&Terminal Name=" +guid+ "&Author email address=ccc&Author github ID=bbb&Description of Activity Functionality=ccc"),
            });


            await client.PostAsync(new Uri(url), formData);

            //Submitting test form directly
            //var googleFormContent = new FormUrlEncodedContent(new[]
            //  {
            //        new KeyValuePair<string, string>("entry.1065502632","aaa"),
            //        new KeyValuePair<string, string>("entry.817648369",guidTestId.ToString()),
            //        new KeyValuePair<string, string>("entry.1628447134","ccc"),
            //        new KeyValuePair<string, string>("entry.1414260391","bbb"),
            //        new KeyValuePair<string, string>("entry.334183611","ccc"),
            //    });
            //await client.PostAsync(new Uri("https://docs.google.com/forms/d/1YrkG6lxbpp9TjGsW-Hx9x33o6DzOruGGtXpl3hsuSzY/formResponse"), googleFormContent);
        }

        private string AddTokens()
        {
            string userId = "";
            var tokenADO = new AuthorizationTokenDO()
            {
                Token = jiraToken,
                ExternalAccountId = "fr8_atlassian_test@fr8.co",
                CreateDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1),
                Id = Guid.NewGuid()
            };

            var tokenGDO = new AuthorizationTokenDO()
            {
                Token = @"{""AccessToken"":""ya29.CjHXAnhqySXYWbq-JE3Nqpq18L_LGYw3xx_T-lD6jeQd6C2mMoKzQhTWRWFSkPcX-pH_"",""RefreshToken"":""1/ZmUihiXxjwiVd-kQe46hDXKB95VaHM5yP-6bfrS-EUUMEudVrK5jSpoR30zcRFq6"",""Expires"":""2017-11-28T13:29:12.653075+05:00""}",
                ExternalAccountId = "fr8test1@gmail.com",
                CreateDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1),
                Id = Guid.NewGuid()
            };

            var tokenSDO = new AuthorizationTokenDO()
            {
                Token = slackToken,
                ExternalAccountId = "not_user",
                ExternalAccountName = "not_user",
                ExternalDomainId = "T07F83QLE",
                ExternalDomainName = "Fr8",
                CreateDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1),
                Id = Guid.NewGuid()
            };
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                userId = uow.UserRepository.GetAll().Where(u => u.UserName == "integration_test_runner@fr8.company").FirstOrDefault().Id;
                tokenADO.UserID = userId;
                tokenGDO.UserID = userId;
                tokenSDO.UserID = userId;
                tokenADO.TerminalID = uow.TerminalRepository.FindOne(t => t.Name == "terminalAtlassian").Id;
                tokenGDO.TerminalID = uow.TerminalRepository.FindOne(t => t.Name == "terminalGoogle").Id;
                tokenSDO.TerminalID = uow.TerminalRepository.FindOne(t => t.Name == "terminalSlack").Id;
                uow.AuthorizationTokenRepository.Add(tokenADO);
                uow.SaveChanges();
                uow.AuthorizationTokenRepository.Add(tokenGDO);
                uow.SaveChanges();
                uow.AuthorizationTokenRepository.Add(tokenSDO);
                uow.SaveChanges();
            }
            return userId;
        }

        private Jira CreateRestClient(string token)
        {
            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(token);
            credentialsDTO.Domain = credentialsDTO.Domain.Replace("http://", "https://");
            if (!credentialsDTO.Domain.StartsWith("https://"))
            {
                credentialsDTO.Domain = "https://" + credentialsDTO.Domain;
            }
            return Jira.CreateRestClient(credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
        }

        private async Task<ActivityPayload> GetPayload(Guid activityId)
        {
            var activity = await RestfulServiceClient.GetAsync(new Uri(GetHubApiBaseUrl()+ "/activities/get/" + activityId));
            var DTO = JsonConvert.DeserializeObject<ActivityDTO>(activity);
            return Mapper.Map<ActivityPayload>(DTO);
        }

        private async Task ConfigureJira(Guid activityId, string userId)
        {
            var payloadJira = await GetPayload(activityId);
            SetDDL(payloadJira, "AvailableProjects", "fr8test");
            DeleteSprint(payloadJira);
            var DTO = Mapper.Map<ActivityDTO>(payloadJira);
            await RestfulServiceClient.PostAsync(new Uri(GetHubApiBaseUrl() + "activities/configure"), DTO, null, await GetHMACHeader<ActivityDTO>(new Uri(GetHubApiBaseUrl() + "activities/configure"), userId, DTO));
        }

        private void DeleteSprint(ActivityPayload payload)
        {
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var sprints = (DropDownList)crates.FindByName("Sprint");
            sprints.Value = null;
            sprints.selectedKey = null;
            crates.Controls.Remove(crates.Controls.Last());
        }

        private async Task ConfigureSlack(Guid activityId, string userId)
        {
            var payloadSlack = await GetPayload(activityId);
            var slackCrates = payloadSlack.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();

            SetDDL(payloadSlack, slackCrates.Controls[0].Name, "#general");
            var DTO = Mapper.Map<ActivityDTO>(payloadSlack);

            await RestfulServiceClient.PostAsync(new Uri(GetHubApiBaseUrl()+ "activities/save"), DTO, null, await GetHMACHeader(new Uri(GetHubApiBaseUrl() + "activities/save"), userId, DTO));
        }

        private async Task ConfigureMessage(Guid activityId, string userId,string guid)
        {
            var payloadMessage = await GetPayload(activityId);
            var messageCrates = payloadMessage.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var bodyTextBox = (BuildMessageAppender)messageCrates.FindByName("Body");
            bodyTextBox.Value = "testing terminal submission " + guid;
            var DTO = Mapper.Map<ActivityDTO>(payloadMessage);
            await RestfulServiceClient.PostAsync(new Uri(GetHubApiBaseUrl()+ "activities/configure"), DTO, null, await GetHMACHeader<ActivityDTO>(new Uri(GetHubApiBaseUrl() + "activities/configure"), userId, DTO));
        }

        private void SetDDL(ActivityPayload payload, string name, string key)
        {
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var ddl = (DropDownList)crates.FindByName(name);
            ddl.SelectByKey(key);
        }
    }
}
