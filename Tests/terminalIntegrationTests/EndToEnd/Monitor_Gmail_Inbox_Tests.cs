using Data.Entities;
using Data.Interfaces;
using Data.Repositories.SqlBased;
using Data.States;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Integration;
using Fr8.Testing.Integration.Tools.Activities;
using Fr8.Testing.Integration.Tools.Plans;
using Fr8.Testing.Unit.Fixtures;
using Hangfire;
using Hub.Interfaces;
using Hub.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using terminalUtilities.Models;
using terminalUtilities.SendGrid;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class Monitor_Gmail_Inbox_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName => "terminalGoogle";
        private readonly IntegrationTestTools _plansHelper;
        private readonly IntegrationTestTools_terminalGoogle _googleActivityConfigurator;
        private readonly IntegrationTestTools_terminalFr8 _fr8ActivityConfigurator;
        private const int MaxAwaitPeriod = 90000;
        private const int SingleAwaitPeriod = 5000;
        private string GoogleEmail = "icantcomeupwithauniquename@gmail.com";
        private string PlanName = "GmailMonitorTestPlan";

        public Monitor_Gmail_Inbox_Tests()
        {
            _googleActivityConfigurator = new IntegrationTestTools_terminalGoogle(this);
            _plansHelper = new IntegrationTestTools(this);
            _fr8ActivityConfigurator = new IntegrationTestTools_terminalFr8(this);
        }

        // this test requires internet connection
        [Test ,Category("Integration.terminalGoogle")]
        public async Task Monitor_Gmail_Inbox_Test()
        {
            Fr8AccountDO currentUser = null;
            AuthorizationTokenDO token = null;
            //we use a separate google account for this test. 
            GetDifferentGoogleAuthToken(out currentUser, out token);

            var testPlan = await _plansHelper.CreateNewPlan(PlanName);

            await AddAndAuthorizeMonitorGmailInboxActivity(token, testPlan);
            //add saveToFr8Warehouse activity
            var saveToFr8WarehouseActivity = await _fr8ActivityConfigurator.AddAndConfigureSaveToFr8Warehouse(testPlan, 2);
            SetCratesForSaving(saveToFr8WarehouseActivity);
            saveToFr8WarehouseActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(GetHubApiBaseUrl() + "activities/configure", saveToFr8WarehouseActivity);

            //run the plan
            await _plansHelper.RunPlan(testPlan.Id);
            await Task.Delay(10000);

            //sending an email
            SendAnEmailToMonitoredAccountViaGoogle();

            //testing
            var stopwatch = Stopwatch.StartNew();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var mtDataCountBefore = uow.MultiTenantObjectRepository
               .AsQueryable<StandardPayloadDataCM>(currentUser.Id.ToString())
               .Count();
                int mtDataCountAfter = mtDataCountBefore;
                while (stopwatch.ElapsedMilliseconds <= MaxAwaitPeriod)
                {
                    await Task.Delay(SingleAwaitPeriod);
                    mtDataCountAfter = uow.MultiTenantObjectRepository
                        .AsQueryable<StandardEmailMessageCM>(currentUser.Id.ToString()).Count();

                    if (mtDataCountBefore < mtDataCountAfter)
                    {
                        break;
                    }
                }
                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter,
                  $"The number of MtData: ({mtDataCountAfter}) records for user {currentUser.UserName} remained unchanged within {MaxAwaitPeriod} miliseconds.");
            }

            //left here in case the test is ran locally, so the plans don't stack
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plans = uow.PlanRepository.GetPlanQueryUncached().Where(a => a.Name == PlanName && a.Fr8AccountId == currentUser.Id).ToList();
                foreach (var plan in plans)
                {
                    await HttpDeleteAsync(GetHubApiBaseUrl() + $"/plans?id={plan.Id}");
                }
            }

            //cleaning hangfire job
            string connString = (string)ObjectFactory.GetInstance<ISqlConnectionProvider>().ConnectionInfo;

            JobStorage.Current = new Hangfire.SqlServer.SqlServerStorage(connString);
            string terminalSecret = "";
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalSecret = uow.TerminalRepository.GetQuery().Where(a => a.Name == "terminalGoogle").FirstOrDefault().Secret;
            }
            string jobId = terminalSecret + "|" + token.ExternalAccountId;
            RecurringJob.RemoveIfExists(jobId);
        }

        private static void SendAnEmailToMonitoredAccountViaGoogle()
        {
            SmtpClient client = new SmtpClient();
            client.Credentials = new NetworkCredential("icantcomeupwithauniquename@gmail.com", "grolier34");
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;


            MailAddress
                maFrom = new MailAddress("icantcomeupwithauniquename@gmail.com", "Sender's Name", Encoding.UTF8), maTo = new MailAddress("icantcomeupwithauniquename@gmail.com", "Recipient's Name", Encoding.UTF8);
            MailMessage mmsg = new MailMessage(maFrom.Address, maTo.Address);
            mmsg.Body = "<html><body><h1>Some HTML Text for Test as BODY</h1></body></html>";
            mmsg.BodyEncoding = Encoding.UTF8;
            mmsg.IsBodyHtml = true;
            mmsg.Subject = "Some Other Text as Subject";
            mmsg.SubjectEncoding = Encoding.UTF8;

            client.Send(mmsg);
        }

        private async Task AddAndAuthorizeMonitorGmailInboxActivity(AuthorizationTokenDO token, PlanDTO testPlan)
        {
            var gmailMonitorActivity = await _googleActivityConfigurator.CreateMonitorGmailInbox(testPlan, 1);
            gmailMonitorActivity.CrateStorage = new CrateStorageDTO();
            var tokenDTO = AutoMapper.Mapper.Map<AuthorizationTokenDTO>(token);

            var applyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = gmailMonitorActivity.Id,
                AuthTokenId = Guid.Parse(tokenDTO.Id),
                IsMain = false
            };
            await HttpPostAsync<AuthenticationTokenGrantDTO[], string>(GetHubApiBaseUrl() + "authentication/tokens/grant", new[] { applyToken });
            gmailMonitorActivity = await _googleActivityConfigurator.SaveActivity(gmailMonitorActivity);
            gmailMonitorActivity = await _googleActivityConfigurator.ConfigureActivity(gmailMonitorActivity);
        }

        private void GetDifferentGoogleAuthToken(out Fr8AccountDO currentUser, out AuthorizationTokenDO token)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                currentUser = uow.UserRepository.GetQuery().Where(a => a.UserName == TestUserEmail).FirstOrDefault();

                var googleTerminalId = uow.TerminalRepository.FindOne(t => t.Name.Equals("terminalGoogle")).Id;
                token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(GoogleEmail, googleTerminalId, currentUser.Id);
                if (token == null)
                {
                    token = new AuthorizationTokenDO();
                    token.Token = FixtureData.GetGoogleAuthorizationTokenForGmailMonitor();
                    token.CreateDate = DateTime.Now;
                    token.LastUpdated = DateTime.Now;
                    token.TerminalID = googleTerminalId;
                    token.UserID = currentUser.Id;
                    token.ExternalAccountId = GoogleEmail;
                    uow.AuthorizationTokenRepository.Add(token);
                    uow.SaveChanges();
                    token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(GoogleEmail, googleTerminalId, currentUser.Id);
                }
            }
        }

        private void SetCratesForSaving(ActivityDTO saveToFr8WarehouseActivity)
        {
            CrateStorageDTO crateStorageDTO = saveToFr8WarehouseActivity.CrateStorage;
            var _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            var crateStorage = _crateManager.FromDto(crateStorageDTO);
            var configControlCM = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .First();
            var upstreamCrateChooser = (UpstreamCrateChooser)configControlCM.FindByName("UpstreamCrateChooser");
            var existingDdlbSource = upstreamCrateChooser.SelectedCrates[0].ManifestType.Source;
            var existingLabelDdlb = upstreamCrateChooser.SelectedCrates[0].Label;
            var standardPayloadManifest = new DropDownList
            {
                selectedKey = Fr8.Infrastructure.Data.Constants.MT.StandardEmailMessage.ToString(),
                Value = ((int)Fr8.Infrastructure.Data.Constants.MT.StandardEmailMessage).ToString(),
                Name = "UpstreamCrateChooser_mnfst_dropdown_0",
                Source = existingDdlbSource
            };

            upstreamCrateChooser.SelectedCrates = new List<CrateDetails>
            {
                new CrateDetails { ManifestType = standardPayloadManifest, Label = existingLabelDdlb }
            };

            saveToFr8WarehouseActivity.CrateStorage = _crateManager.ToDto(crateStorage);
        }

    }
}
