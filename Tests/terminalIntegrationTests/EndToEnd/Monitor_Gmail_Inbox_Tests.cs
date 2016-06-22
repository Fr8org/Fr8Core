using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Integration;
using Fr8.Testing.Integration.Tools.Activities;
using Fr8.Testing.Integration.Tools.Plans;
using Fr8.Testing.Unit.Fixtures;
using Hub.Interfaces;
using Hub.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [Test, Category("Integration.terminalGoogle")]
        public async Task Monitor_Gmail_Inbox_Test()
        {
            Fr8AccountDO currentUser = null;
            AuthorizationTokenDO token = null;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                currentUser = uow.UserRepository.GetQuery().Where(a => a.UserName == TestUserEmail).FirstOrDefault();
                var googleTerminalId = uow.TerminalRepository.FindOne(t => t.Name.Equals("terminalGoogle")).Id;
                token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(GoogleEmail, googleTerminalId, currentUser.Id);
                if (token == null)
                {
                    token = AutoMapper.Mapper.Map<AuthorizationTokenDO>(FixtureData.GetGoogleAuthorizationTokenForGmailMonitor());
                    token.CreateDate = DateTime.Now;
                    token.LastUpdated = DateTime.Now;
                    token.ExpiresAt = DateTime.Now.AddHours(1);
                    token.TerminalID = googleTerminalId;
                    token.UserID = currentUser.Id;
                    token.ExternalAccountId = GoogleEmail;
                    uow.AuthorizationTokenRepository.Add(token);
                    uow.SaveChanges();
                    token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(GoogleEmail, googleTerminalId, currentUser.Id);
                }
            }



            //Assert.IsNotNull(validToken, "Reading default google token from AuthorizationTokenRepository failed. " +
            //                             "Please provide default account for authenticating terminalGoogle.");



            var testPlan = await _plansHelper.CreateNewPlan(PlanName);
            var gmailMonitorActivity = await _googleActivityConfigurator.CreateMonitorGmailInbox(testPlan, 0);
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

            var saveToFr8WarehouseActivity = await _fr8ActivityConfigurator.AddAndConfigureSaveToFr8Warehouse(testPlan, 1);
            SetCratesForSaving(saveToFr8WarehouseActivity.CrateStorage);
            saveToFr8WarehouseActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(GetHubApiBaseUrl() + "activities/configure", saveToFr8WarehouseActivity);
            await _plansHelper.RunPlan(testPlan.Plan.Id);
            await Task.Delay(30000);

            //////////////////// TRY IT OUT

            SendGridPackager emailClient = new SendGridPackager(new ConfigRepository());
            await emailClient.Send(new TerminalMailerDO()
            {
                Email = new EmailDTO()
                {
                    From = new EmailAddressDTO
                    {
                        Address = "fr8ops@fr8.company",
                        Name = "Fr8 Operations"
                    },

                    Recipients = new List<RecipientDTO>()
                    {
                        new RecipientDTO()
                        {
                            EmailAddress = new EmailAddressDTO(GoogleEmail),
                            EmailParticipantType = EmailParticipantType.To
                        }
                    },
                    Subject = "test",
                    HTMLText = "test",
                },
                Footer = ""
            }
            );

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
                        .AsQueryable<StandardPayloadDataCM>(currentUser.Id.ToString()).Count();

                    if (mtDataCountBefore < mtDataCountAfter)
                    {
                        break;
                    }
                }

                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter,
                  $"The number of MtData: ({mtDataCountAfter}) records for user {currentUser.UserName} remained unchanged within {MaxAwaitPeriod} miliseconds.");
            }





            ///////////////////// CLEAN UP


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plans = uow.PlanRepository.GetPlanQueryUncached().Where(a => a.Name == PlanName && a.Fr8AccountId == currentUser.Id).ToList();
                foreach (var plan in plans)
                {
                    await HttpDeleteAsync(GetHubApiBaseUrl() + $"/plans?id={plan.Id}");
                }
            }
        }

        private void SetCratesForSaving(CrateStorageDTO crateStorageDTO)
        {
            var crateStorage = ObjectFactory.GetInstance<ICrateManager>().FromDto(crateStorageDTO);
            var configControlCM = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .First();
            var upstreamCrateChooser = (UpstreamCrateChooser)configControlCM.FindByName("UpstreamCrateChooser");
            var existingDdlbSource = upstreamCrateChooser.SelectedCrates[0].ManifestType.Source;
            var existingLabelDdlb = upstreamCrateChooser.SelectedCrates[0].Label;
            var docusignEnvelope = new DropDownList
            {
                selectedKey = Fr8.Infrastructure.Data.Constants.MT.StandardPayloadData.ToString(),
                Value = ((int)Fr8.Infrastructure.Data.Constants.MT.StandardPayloadData).ToString(),
                Name = "UpstreamCrateChooser_mnfst_dropdown_0",
                Source = existingDdlbSource
            };

            upstreamCrateChooser.SelectedCrates = new List<CrateDetails>
            {
                new CrateDetails { ManifestType = docusignEnvelope, Label = existingLabelDdlb }
            };
        }

    }
}
