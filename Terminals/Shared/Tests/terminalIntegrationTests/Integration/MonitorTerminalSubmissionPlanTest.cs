using Atlassian.Jira;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Testing.Integration;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Configuration;
using AutoMapper;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Control;
using Fr8.TerminalBase.Infrastructure;

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

        private static string AtlassianAccountId = ConfigurationManager.AppSettings["AtlassianTestAccountId"];
        private static string AtlassianAccountPassword = ConfigurationManager.AppSettings["AtlassianTestAccountPassword"];
        private const int MaxAwaitPeriod = 45000;
        private const int SingleAwaitPeriod = 3000;
        private const int PlanExecutionPeriod = 10000;

        public static string SlackAuthToken
        {
            get
            {
                return ConfigurationManager.AppSettings["SlackAuthToken"];
            }
        }
        public static string SlackExternalDomainId
        {
            get
            {
                return ConfigurationManager.AppSettings["SlackExternalDomainId"];
            }
        }
        public static string AtlassianToken
        {
            get
            {
                return "{\"Terminal\":null,\"Username\":\"" + AtlassianAccountId + "\",\"Password\":\"" + AtlassianAccountPassword + "\",\"Domain\":\"https://maginot.atlassian.net\",\"IsDemoAccount\":false}";
            }
        }
        public static string GoogleAuthToken
        {
            get
            {
                return ConfigurationManager.AppSettings["GoogleTestAccountToken"];
            }
        }
        public static string GoogleTestAccountId
        {
            get
            {
                return ConfigurationManager.AppSettings["GoogleTestAccountId"];
            }
        }
        
        [Test]
        public async Task MonitorTerminalSubmissionPlan()
        {
            AutoMapperBootstrapper.ConfigureAutoMapper();

            var guidTestId = Guid.NewGuid();

            var userId = AddTokens();

            var googleEventUrl = ConfigurationManager.AppSettings["GoogleFormEventWebServerUrl"];

            //Trigger creating Plan
            Debug.WriteLine("Trigger creating Plan");

            var terminalAuthenticationHeader = GetFr8TerminalAuthorizationHeader("terminalGoogle", "1", userId);
            await RestfulServiceClient.PostAsync(new Uri(googleEventUrl), new { fr8_user_id = userId }, null, terminalAuthenticationHeader);

            //Reconfiguring plan activities 
            var url = $"{GetHubApiBaseUrl()}/plans?name=MonitorSubmissionTerminalForm&visibility=2";
            var response = await RestfulServiceClient.GetAsync(new Uri(url), null, terminalAuthenticationHeader);

            var plans = JsonConvert.DeserializeObject<IEnumerable<PlanDTO>>(response).ToArray();
            var plan = plans.FirstOrDefault().SubPlans.FirstOrDefault();

            // deactivate plan before editing
            Debug.WriteLine("deactivate plan before editing");
            var deactivateUrl = GetHubApiBaseUrl() + "plans/deactivate?planId=" + plans.FirstOrDefault().Id;
            await RestfulServiceClient.PostAsync(new Uri(deactivateUrl), new List<CrateDTO>(), null, terminalAuthenticationHeader);

            Debug.WriteLine("Reconfiguring plan activities");

            if (plan.Activities.FirstOrDefault(a => a.Ordering == 8) != null)
            {
                var deleteActivityUrl = GetHubApiBaseUrl() + "activities/delete/" + plan.Activities.Where(a => a.Ordering == 8).FirstOrDefault().Id;
                await RestfulServiceClient.DeleteAsync(new Uri(deleteActivityUrl), null, terminalAuthenticationHeader);
            }

            await ConfigureJira(plan.Activities.FirstOrDefault(a => a.Ordering == 5).Id, userId);
            await ConfigureMessage(plan.Activities.FirstOrDefault(a => a.Ordering == 6).Id, userId, guidTestId.ToString());
            await ConfigureSlack(plan.Activities.FirstOrDefault(a => a.Ordering == 7).Id, userId);

            //Run plan again after reconfigure
            Debug.WriteLine("Run plan again after reconfigure");
            var runUrl = GetHubApiBaseUrl() + "plans/run?planId=" + plans.FirstOrDefault().Id;
            await RestfulServiceClient.PostAsync(new Uri(runUrl), new List<CrateDTO>(), null, terminalAuthenticationHeader);

            await SubmitForm(googleEventUrl, guidTestId.ToString());

            //Waiting 10 seconds for Plan execution
            Debug.WriteLine("Waiting 10 seconds for Plan execution");
            await Task.Delay(PlanExecutionPeriod);


            Jira jira = CreateRestClient(AtlassianToken);
            Issue[] issues = new Issue[0];

            var slackUrl = "https://slack.com/api/search.messages?token=" + SlackAuthToken + "&query=" + guidTestId.ToString();
            var totalMessagesFound = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds <= MaxAwaitPeriod)
            {
                if (issues.Length == 0)
                {
                    //Searching for created jira issue
                    issues = jira.GetIssuesFromJql("summary ~ " + guidTestId.ToString()).ToArray();
                    Debug.WriteLine("found jira issues " + issues.Length + " after elapsed " + stopwatch.ElapsedMilliseconds + " milliseconds");
                }

                if(totalMessagesFound == 0)
                {
                    //Searching for slack message
                    var result = await RestfulServiceClient.GetAsync(new Uri(slackUrl));
                    var searchResult = JObject.Parse(result);
                    totalMessagesFound = (int)searchResult.SelectToken("messages.pagination.total_count");
                    Debug.WriteLine("found slack messages " + totalMessagesFound + " after elapsed " + stopwatch.ElapsedMilliseconds + " milliseconds");
                }

                if (issues.Count() != 0 && totalMessagesFound != 0)
                {
                    break;
                }
                await Task.Delay(SingleAwaitPeriod);
            }

            //Deleting test issues
            foreach (var issue in issues)
            {
                jira.DeleteIssue(issue);
            }

            Assert.IsTrue(issues.Length > 0,"Couldn't find jira issue");
            
            Assert.IsTrue(totalMessagesFound != 0,"Couldn't find slack message");
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
                Token = AtlassianToken,
                ExternalAccountId = AtlassianAccountId,
                CreateDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1),
                Id = Guid.NewGuid()
            };

            var tokenGDO = new AuthorizationTokenDO()
            {
                Token = GoogleAuthToken,
                ExternalAccountId = GoogleTestAccountId,
                CreateDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1),
                Id = Guid.NewGuid()
            };

            var tokenSDO = new AuthorizationTokenDO()
            {
                Token = SlackAuthToken,
                ExternalAccountId = "not_user",
                ExternalAccountName = "not_user",
                ExternalDomainId = SlackExternalDomainId,
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
                var tokenA = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(tokenADO.ExternalAccountId, tokenADO.TerminalID, userId);
                if (tokenA == null)
                {
                    uow.AuthorizationTokenRepository.Add(tokenADO);
                    uow.SaveChanges();
                }

                var tokenG = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(tokenGDO.ExternalAccountId, tokenGDO.TerminalID, userId);
                if (tokenG == null)
                {
                    uow.AuthorizationTokenRepository.Add(tokenGDO);
                    uow.SaveChanges();
                }

                var tokenS = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(tokenSDO.ExternalAccountId, tokenSDO.TerminalID, userId);
                if (tokenS == null)
                {
                    uow.AuthorizationTokenRepository.Add(tokenSDO);
                    uow.SaveChanges();
                }
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
            var activity = await RestfulServiceClient.GetAsync(new Uri(GetHubApiBaseUrl()+ "activities/get/" + activityId));
            var DTO = JsonConvert.DeserializeObject<ActivityDTO>(activity);
            return Mapper.Map<ActivityPayload>(DTO);
        }

        private async Task ConfigureJira(Guid activityId, string userId)
        {
            var payloadJira = await GetPayload(activityId);
            SetDDL(payloadJira, "AvailableProjects", "fr8test");
            DeleteSprint(payloadJira);
            var DTO = Mapper.Map<ActivityDTO>(payloadJira);
            await RestfulServiceClient.PostAsync(new Uri(GetHubApiBaseUrl() + "activities/configure"), DTO, null, GetFr8HubAuthorizationHeader("terminalGoogle", "1", userId));
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

            await RestfulServiceClient.PostAsync(new Uri(GetHubApiBaseUrl()+ "activities/save"), DTO, null, GetFr8HubAuthorizationHeader("terminalGoogle", "1", userId));
        }

        private async Task ConfigureMessage(Guid activityId, string userId,string guid)
        {
            var payloadMessage = await GetPayload(activityId);
            var messageCrates = payloadMessage.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var bodyTextBox = (BuildMessageAppender)messageCrates.FindByName("Body");
            bodyTextBox.Value = "testing terminal submission " + guid;
            var DTO = Mapper.Map<ActivityDTO>(payloadMessage);
            await RestfulServiceClient.PostAsync(new Uri(GetHubApiBaseUrl()+ "activities/configure"), DTO, null, GetFr8HubAuthorizationHeader("terminalGoogle", "1", userId));
        }

        private void SetDDL(ActivityPayload payload, string name, string key)
        {
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var ddl = (DropDownList)crates.FindByName(name);
            ddl.SelectByKey(key);
        }
    }
}

