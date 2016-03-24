using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using HealthMonitor.Utility;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Services.New_Api;
using terminalIntegrationTests.Fixtures;

namespace terminalIntegrationTests.Helpers
{
    public class ActivityConfigurator
    {
        private BaseHubIntegrationTest baseHubIntTest;
        public ActivityConfigurator(BaseHubIntegrationTest baseIntTest)
        {
            baseHubIntTest = baseIntTest;
        }

        #region Plan Methods
        public async Task<PlanDTO> CreateNewPlan()
        {
            var newPlan = HealthMonitor_FixtureData.TestPlanEmptyDTO();

            var planDTO = await baseHubIntTest.HttpPostAsync<PlanEmptyDTO, PlanDTO>(baseHubIntTest.GetHubApiBaseUrl() + "plans", newPlan);

            Assert.True(planDTO.Plan.SubPlans.Any(), "New created Plan doesn't have an existing sub plan.");

            return await Task.FromResult(planDTO);
        }

        #endregion

        #region terminalDocuSign Activities

        public async Task<ActivityDTO> AddAndConfigure_QueryDocuSign(PlanDTO plan, int ordering)
        {
            var queryDocuSignActivity = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO().ActivityDTO;
            queryDocuSignActivity.ActivityTemplate.Terminal = GetTerminal("terminalDocuSign",1);
            queryDocuSignActivity.ActivityTemplate.TerminalId = queryDocuSignActivity.ActivityTemplate.Terminal.Id;

            //connect current activity with a plan
            var subPlan = plan.Plan.SubPlans.FirstOrDefault();
            queryDocuSignActivity.ParentPlanNodeId = subPlan.SubPlanId;
            queryDocuSignActivity.RootPlanNodeId = plan.Plan.Id;
            queryDocuSignActivity.Ordering = ordering;

            //call initial configuration to server
            queryDocuSignActivity = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/save", queryDocuSignActivity);
            //this call is without authtoken
            queryDocuSignActivity = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/configure?", queryDocuSignActivity);

            var initialcrateStorage = baseHubIntTest.Crate.FromDto(queryDocuSignActivity.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaultDocuSignAuthTokenExists = stAuthCrate == null;

            if (!defaultDocuSignAuthTokenExists)
            {
                queryDocuSignActivity.AuthToken = await DocuSign_AuthToken(queryDocuSignActivity.ActivityTemplate.TerminalId);

                var applyToken = new ManageAuthToken_Apply()
                {
                    ActivityId = queryDocuSignActivity.Id,
                    AuthTokenId = Guid.Parse(queryDocuSignActivity.AuthToken.Token),
                    IsMain = true
                };
                await baseHubIntTest.HttpPostAsync<ManageAuthToken_Apply[], string>(baseHubIntTest.GetHubApiBaseUrl() + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken });
            }

            //send configure with the auth token
            queryDocuSignActivity = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/configure?", queryDocuSignActivity);
            initialcrateStorage = baseHubIntTest.Crate.FromDto(queryDocuSignActivity.CrateStorage);
            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), "Crate StandardConfigurationControlsCM is missing in API response.");

            var controlsCrate = initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            //set the value of folder to drafts and 
            var controls = controlsCrate.Content.Controls;
            var folderControl = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "Folder");
            folderControl.Value = "Draft";
            folderControl.selectedKey = "Draft";
            //set the value of status to any
            var statusControl = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "Status");
            statusControl.Value = null;
            statusControl.selectedKey = null;

            //call followup configuration
            using (var crateStorage = baseHubIntTest.Crate.GetUpdatableStorage(queryDocuSignActivity))
            {
                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Add(controlsCrate);
            }
            queryDocuSignActivity = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/save", queryDocuSignActivity);
            queryDocuSignActivity = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/configure", queryDocuSignActivity);

            return await Task.FromResult(queryDocuSignActivity);
        }

        #endregion

        #region terminalGoogle Activities

        public async Task<ActivityDTO> AddAndConfigure_SaveToGoogleSheet(PlanDTO plan, int ordering, string manufestTypeToAssert, string crateDescriptionLabelToAssert)
        {
            var saveToGoogleActivity = HealthMonitor_FixtureData.Save_To_Google_Sheet_v1_InitialConfiguration_Fr8DataDTO();
            saveToGoogleActivity.ActivityDTO.ActivityTemplate.Terminal = GetTerminal("terminalGoogle",1);
            saveToGoogleActivity.ActivityDTO.ActivityTemplate.TerminalId = saveToGoogleActivity.ActivityDTO.ActivityTemplate.Terminal.Id;

            //connect current activity with a plan
            var subPlan = plan.Plan.SubPlans.FirstOrDefault();
            saveToGoogleActivity.ActivityDTO.ParentPlanNodeId = subPlan.SubPlanId;
            saveToGoogleActivity.ActivityDTO.RootPlanNodeId = plan.Plan.Id;
            saveToGoogleActivity.ActivityDTO.Ordering = ordering;

            //call initial configuration to server
            saveToGoogleActivity.ActivityDTO = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/save", saveToGoogleActivity.ActivityDTO);
            saveToGoogleActivity.ActivityDTO = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/configure?", saveToGoogleActivity.ActivityDTO);
            var initialcrateStorage = baseHubIntTest.Crate.FromDto(saveToGoogleActivity.ActivityDTO.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaultDocuSignAuthTokenExists = stAuthCrate == null;

            if (!defaultDocuSignAuthTokenExists)
            {
                saveToGoogleActivity.ActivityDTO.AuthToken = HealthMonitor_FixtureData.Google_AuthToken();//await Google_AuthToken(saveToGoogleActivity.ActivityTemplate.TerminalId);

                //var applyToken = new ManageAuthToken_Apply()
                //{
                //    ActivityId = saveToGoogleActivity.Id,
                //    AuthTokenId = Guid.Parse(saveToGoogleActivity.AuthToken.Id),
                //    IsMain = true
                //};
                //await HttpPostAsync<ManageAuthToken_Apply[], string>(_baseUrl + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken });
            }

            saveToGoogleActivity.ActivityDTO =
                await
                    baseHubIntTest.HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                        NormalizeSchema(saveToGoogleActivity.ActivityDTO.ActivityTemplate.Terminal.Endpoint) +
                        "/activities/configure", saveToGoogleActivity);


            //saveToGoogleActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?", saveToGoogleActivity);
            initialcrateStorage = baseHubIntTest.Crate.FromDto(saveToGoogleActivity.ActivityDTO.CrateStorage);
            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), "Crate StandardConfigurationControlsCM is missing in API response.");

            var controlsCrate = initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            //set the value of folder to drafts and 
            var controls = controlsCrate.Content.Controls;

            var crateChooser = controls
                .Where(x => x.Type == ControlTypes.CrateChooser && x.Name == "UpstreamCrateChooser")
                .SingleOrDefault() as CrateChooser;

            Assert.NotNull(crateChooser);
            Assert.AreEqual(1, crateChooser.CrateDescriptions.Count);
            Assert.AreEqual(manufestTypeToAssert, crateChooser.CrateDescriptions[0].ManifestType);
            Assert.AreEqual(crateDescriptionLabelToAssert, crateChooser.CrateDescriptions[0].Label);

            //select the first crate description
            using (var crateStorage = baseHubIntTest.Crate.GetUpdatableStorage(saveToGoogleActivity.ActivityDTO))
            {
                crateChooser.CrateDescriptions.First().Selected = true;
            }

            saveToGoogleActivity.ActivityDTO = await baseHubIntTest.HttpPostAsync<ActivityDTO, ActivityDTO>(baseHubIntTest.GetHubApiBaseUrl() + "activities/save", saveToGoogleActivity.ActivityDTO);
            saveToGoogleActivity.ActivityDTO = await baseHubIntTest.HttpPostAsync<Fr8DataDTO, ActivityDTO>(NormalizeSchema(saveToGoogleActivity.ActivityDTO.ActivityTemplate.Terminal.Endpoint) +
                       "/activities/configure", saveToGoogleActivity);

            return await Task.FromResult(saveToGoogleActivity.ActivityDTO);
        }

        #endregion

        #region AuthToken Management

        public async Task<AuthorizationTokenDTO> DocuSign_AuthToken(int terminalId)
        {
            var creds = new CredentialsDTO()
            {
                Username = "fr8test@gmail.com",
                Password = "fr8mesomething",
                IsDemoAccount = true,
                TerminalId = terminalId
            };
            var token = await baseHubIntTest.HttpPostAsync<CredentialsDTO, JObject>(baseHubIntTest.GetHubApiBaseUrl()+ "authentication/token", creds);
            Assert.AreNotEqual(token["error"].ToString(), "Unable to authenticate in DocuSign service, invalid login name or password.", "DocuSign auth error. Perhaps max number of tokens is exceeded.");
            Assert.AreEqual(false, String.IsNullOrEmpty(token["authTokenId"].Value<string>()), "AuthTokenId is missing in API response.");
            Guid tokenGuid = Guid.Parse(token["authTokenId"].Value<string>());

            return await Task.FromResult(new AuthorizationTokenDTO { Token = tokenGuid.ToString() });
        }

        public async Task<AuthorizationTokenDTO> Google_AuthToken(int terminalId)
        {
            //var endPoint = GetHubBaseUrl() + "authenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalGoogle&terminalVersion=1&state=67be8cd5-532a-4241-b6a3-77e2eecd7ff6&code=4/qHIUiVkyT4JvkxKQgptcnSGF2iCPGF0BDnJGi4g7u28";
            //await HttpGetAsync<ActionResult>(endPoint);

            TerminalDO terminal = ObjectFactory.GetInstance<ITerminal>().GetAll().FirstOrDefault(x => x.Id == terminalId);

            if (terminal == null)
            {
                throw new ApplicationException("Could not find terminal.");
            }

            var externalAuthenticationDTO = new ExternalAuthenticationDTO()
            {
                RequestQueryString = "terminalName=terminalGoogle&terminalVersion=1&state=b31cc220-9cb8-4bba-9b31-3c8ecf246a9d&code=4/PAUy56kfwQ3BQV1ZHm_h-FDyR9n0QTPAYLbBSmszCwc"
            };

            var response = await ObjectFactory.GetInstance<IAuthorization>().GetOAuthToken(terminal, externalAuthenticationDTO);

            var authTokens = await baseHubIntTest.HttpGetAsync<List<ManageAuthToken_Terminal>>(baseHubIntTest.GetHubApiBaseUrl() + "ManageAuthToken");
            var authToken = authTokens.FirstOrDefault(a => a.Name.Equals("terminalGoogle"));
            var mainAuthToken = new AuthorizationTokenDTO { Id = authToken.AuthTokens.FirstOrDefault(x => x.ExternalAccountName == "fr8test@gmail.com").Id.ToString() };

            return await Task.FromResult(mainAuthToken);
        }

        #endregion

        #region Private Methods 

        public TerminalDTO GetTerminal(string terminalName, int terminalversion)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminal = uow.TerminalRepository.GetQuery()
                    .FirstOrDefault(t => t.Version == terminalversion.ToString() && t.Name == terminalName);
                if (null == terminal)
                {
                    throw new Exception(
                        String.Format("Terminal with name {0} and version {1} not found", terminalName, terminalversion));
                }
                return Mapper.Map<TerminalDO, TerminalDTO>(terminal);
            }
        }

        public static string NormalizeSchema(string endpoint)
        {
            if (endpoint.StartsWith("http://"))
            {
                return endpoint;
            }
            else if (endpoint.StartsWith("https://"))
            {
                return endpoint.Replace("https://", "http://");
            }
            else
            {
                return "http://" + endpoint;
            }
        }

        public EnvelopeSummary CreateNewDocuSignEnvelope(AuthorizationTokenDO authorizationToken)
        {
            var loginInfo = new DocuSignManager().SetUp(authorizationToken);

            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "IntTest message from Fr8";
            envDef.TemplateId = "1A380ECC-6759-4473-A1A4-9DC544F2DA24";
            envDef.Status = "created";

            //creating an envelope
            EnvelopesApi envelopesApi = new EnvelopesApi(loginInfo.Configuration);
            return envelopesApi.CreateEnvelope(loginInfo.AccountId, envDef);
        }

        #endregion
    }
}
