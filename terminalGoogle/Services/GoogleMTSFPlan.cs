using Data.States;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using log4net;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace terminalGoogle.Services
{
    public class GoogleMTSFPlan
    {
        private string _userId;
        private IHubCommunicator _hubCommunicator;
        private string[] _slackChannels;
        private int activitiesCount = 0;
        private PlanDTO monitorTerminalSubmissions;
        private ActivityTemplateDTO monitorFormResponsesTmpl, buildMessageTmpl, saveJiraIssueTmpl, publishToSlackTmpl;
        private AuthenticationTokenTerminalDTO googleTokens, atlassianTokens, slackTokens;
        private static readonly ILog Logger = LogManager.GetLogger("terminalGoogle");

        public GoogleMTSFPlan(string userId,IHubCommunicator hubCommunicator, params string[] slackChannels)
        {
            _userId = userId;
            _hubCommunicator = hubCommunicator;
            _slackChannels = slackChannels;

        }
        public async Task PlanConfiguration()
        {
            await InitialPlanConfiguration();
            await CreateAndConfigureGoogle("ga_admin@fr8.co");
            await CreateAndConfigureJiraMessages();
            await CreateAndConfigureSaveToJiraActivity("fr8test");
            await CreateAndConfigureSlackMessage("Created new jira issue for terminal submission: [jira issue key],  [jira domain]/browse/[jira issue key]");
            foreach (var channel in _slackChannels)
            {
                await CreateAndConfigureSlackActivity(channel);
            }
        }

        public async Task InitialPlanConfiguration()
        {
            var emptyMonitorPlan = new PlanNoChildrenDTO
            {
                Name = "MonitorSubmissionTerminalForm",
                Description = "MonitorSubmissionTerminalForm",
                PlanState = "Active",
                Visibility = new PlanVisibilityDTO() { Hidden = true }
            };

            monitorTerminalSubmissions = await _hubCommunicator.CreatePlan(emptyMonitorPlan);
            var activityTemplates = await _hubCommunicator.GetActivityTemplates(null);
            monitorFormResponsesTmpl = GetActivityTemplate(activityTemplates, "Monitor_Form_Responses");
            buildMessageTmpl = GetActivityTemplate(activityTemplates, "Build_Message");
            saveJiraIssueTmpl = GetActivityTemplate(activityTemplates, "Save_Jira_Issue");
            publishToSlackTmpl = GetActivityTemplate(activityTemplates, "Publish_To_Slack", "2");
        }

        public async Task CreateAndConfigureGoogle(string name)
        {
            activitiesCount++;
            var googleToken = googleTokens.AuthTokens.Where(t => t.ExternalAccountName == name).FirstOrDefault() != null ? googleTokens.AuthTokens.Where(t => t.ExternalAccountName == name).FirstOrDefault() : googleTokens.AuthTokens.FirstOrDefault();
            var monitorGoogle = await _hubCommunicator.CreateAndConfigureActivity(monitorFormResponsesTmpl.Id, "Monitor Terminal Submission Form", activitiesCount, monitorTerminalSubmissions.StartingSubPlanId, false, googleToken.Id);
            SetDDL(monitorGoogle, "Selected_Google_Form", "Terminal Submission Form");
            await _hubCommunicator.ConfigureActivity(monitorGoogle);
        }

        public async Task CreateAndConfigureJiraMessages()
        {
            activitiesCount++;
            await CreateAndConfigureMessageActivity(buildMessageTmpl.Id,
                 "Message for email", activitiesCount, monitorTerminalSubmissions.StartingSubPlanId, "mail", "mailto:[Author email address]");
            activitiesCount++;
            await CreateAndConfigureMessageActivity(buildMessageTmpl.Id,
                 "Jira description", activitiesCount, monitorTerminalSubmissions.StartingSubPlanId,
                "jira description",
                "*Github Pull Request URL*: [Github Pull Request URL] \\\\ *Terminal Name*: [Terminal Name] \\\\ *Author email address*: [mail] \\\\ *Author github ID*: [Author github ID] \\\\ *Description of Activity Functionality*:[Description of Activity Functionality]");

            activitiesCount++;
            await CreateAndConfigureMessageActivity(buildMessageTmpl.Id,
                 "Jira summary", activitiesCount, monitorTerminalSubmissions.StartingSubPlanId, "jira summary", "Terminal submission for [Terminal Name]");
        }

        public async Task CreateAndActivateNewMTSFPlan()
        {
            try
            {
                Logger.Info("Star Creating Plan");
                var plans = await _hubCommunicator.GetPlansByName("MonitorSubmissionTerminalForm", PlanVisibility.Internal);
                var tokens = await _hubCommunicator.GetTokens();

                googleTokens = tokens.Where(t => t.Name == "terminalGoogle").FirstOrDefault();
                atlassianTokens = tokens.Where(t => t.Name == "terminalAtlassian").FirstOrDefault();
                slackTokens = tokens.Where(t => t.Name == "terminalSlack").FirstOrDefault();
                if (plans.Count() == 0)
                {
                    await ConfigureAndRunPlan();
                }
                else
                {
                    Logger.Info("Plan already exist");
                    var plan = plans.FirstOrDefault();
                    if (plan.SubPlans.FirstOrDefault().Activities.Count < 8)
                    {
                        Logger.Info("Deleting incomplete Plan");
                        await _hubCommunicator.DeletePlan(plan.Id);

                        await ConfigureAndRunPlan();
                    }
                    Logger.Info("trying to reapply tokens");
                    //Reapllying tokens if they were revoked previously
                    await ReApplyTokens(plans.FirstOrDefault());
                }
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't create MonitorTerminalSubmissionForm Plan", e);
                throw new ApplicationException("Couldn't create MonitorTerminalSubmissionForm Plan", e);
            }
        }

        public async Task ConfigureAndRunPlan()
        {
            await PlanConfiguration();
            Logger.Info("new MonitorTerminalSubmissionPlan created");

            Logger.Info("Run MonitorTerminalSubmissionPlan Plan");
            await RunPlan();
            Logger.Info("MonitorTerminalSubmissionPlan Plan activeted");
        }

        public async Task ReApplyTokens(PlanDTO plan)
        {
            var planMTSF = plan.SubPlans.FirstOrDefault();
            if (planMTSF !=null)
            {
                var googleActivity = planMTSF.Activities.FirstOrDefault();
                if(googleActivity != null)
                {
                    var curentGToken = googleTokens.AuthTokens.Where(t => t.Id == googleActivity.AuthTokenId).FirstOrDefault();
                    if (curentGToken == null)
                    {
                        var gToken = googleTokens.AuthTokens.Where(t => t.ExternalAccountName == "ga_admin@fr8.co").FirstOrDefault();
                        if (gToken != null)
                        {
                            await _hubCommunicator.ApplyNewToken(googleActivity.Id, gToken.Id);
                        }
                    }
                }

                var atlassianActivity = planMTSF.Activities[4];
                if(atlassianActivity != null)
                {
                    var curentAToken = atlassianTokens.AuthTokens.Where(t => t.Id == atlassianActivity.AuthTokenId).FirstOrDefault();
                    if (curentAToken == null)
                    {
                        var aToken = atlassianTokens.AuthTokens.FirstOrDefault();
                        if (aToken != null)
                        {
                            await _hubCommunicator.ApplyNewToken(atlassianActivity.Id, aToken.Id);
                        }
                    }
                }

                foreach (var slack in planMTSF.Activities.Where(term => term.ActivityTemplate.TerminalName == "terminalSlack"))
                {
                    var curentSToken = slackTokens.AuthTokens.FirstOrDefault(t => t.Id == slack.AuthTokenId);
                    if (curentSToken == null)
                    {
                        var sToken = slackTokens.AuthTokens.FirstOrDefault();
                        if (sToken != null)
                        {
                            await _hubCommunicator.ApplyNewToken(slack.Id, sToken.Id);
                        }
                    }
                }
            }
        }

        public async Task RunPlan()
        {
            await _hubCommunicator.RunPlan(monitorTerminalSubmissions.Id, null);
        }

        public async Task CreateAndConfigureSlackActivity(string slackChannel)
        {
            activitiesCount++;
            var slackActivity = await _hubCommunicator.CreateAndConfigureActivity(publishToSlackTmpl.Id, HttpUtility.UrlEncode("post to " + slackChannel), activitiesCount, monitorTerminalSubmissions.StartingSubPlanId, false, slackTokens.AuthTokens.FirstOrDefault().Id);
            SetDDL(slackActivity, slackActivity.CrateStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>().Content.Controls[0].Name, slackChannel);
            var data = await _hubCommunicator.GetAvailableData(slackActivity.Id, CrateDirection.Upstream, AvailabilityType.NotSet);
            SetUpstream(slackActivity, slackActivity.CrateStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>().Content.Controls[1].Name, "slack message", data);
            await _hubCommunicator.SaveActivity(slackActivity);
        }

        public async Task CreateAndConfigureSlackMessage(string message)
        {
            activitiesCount++;
            await CreateAndConfigureMessageActivity(buildMessageTmpl.Id,
                "Message for slack", activitiesCount, monitorTerminalSubmissions.StartingSubPlanId, "slack message", message);
        }

        public async Task CreateAndConfigureSaveToJiraActivity(string jiraProjectName)
        {
            activitiesCount++;
            var saveJira = await _hubCommunicator.CreateAndConfigureActivity(saveJiraIssueTmpl.Id, "Save to jira", activitiesCount, monitorTerminalSubmissions.StartingSubPlanId, false, atlassianTokens.AuthTokens.FirstOrDefault().Id);
            SetDDL(saveJira, "AvailableProjects", jiraProjectName);
            saveJira = await _hubCommunicator.ConfigureActivity(saveJira);
            SetDDL(saveJira, "AvailableIssueTypes", "Improvement");
            saveJira = await _hubCommunicator.ConfigureActivity(saveJira);
            var data = await _hubCommunicator.GetAvailableData(saveJira.Id, CrateDirection.Upstream, AvailabilityType.NotSet);
            SetDDL(saveJira, "AvailablePriorities", "Normal");
            SetSprint(saveJira);
            saveJira = await _hubCommunicator.ConfigureActivity(saveJira);
            SetUpstream(saveJira, "SummaryTextSource", "jira summary", data);
            SetUpstream(saveJira, "DescriptionTextSource", "jira description", data);
            await _hubCommunicator.SaveActivity(saveJira);
        }

        private async Task CreateAndConfigureMessageActivity(Guid tmplId, string activityName, int order, Guid parentNodeId, string messageName, string messageBody)
        {
            var messageActivity = await _hubCommunicator.CreateAndConfigureActivity(tmplId, activityName, order, parentNodeId, false, null);
            SetMessage(messageActivity, messageName, messageBody);
            await _hubCommunicator.ConfigureActivity(messageActivity);
        }

        private void SetUpstream(ActivityPayload payload, string crateLabel, string fieldKey, IncomingCratesDTO data)
        {
            var crate = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var nameTextBox = (TextSource)crate.FindByName(crateLabel);
            var field = data.AvailableCrates.Where(c => c.Fields.Where(f => f.Name == fieldKey).FirstOrDefault() != null).FirstOrDefault().Fields.Where(f => f.Name == fieldKey).FirstOrDefault();
            nameTextBox.ValueSource = "upstream";
            nameTextBox.SelectedItem = field;
            nameTextBox.selectedKey = field.Name;
        }

        private void SetDDL(ActivityPayload payload, string name, string key)
        {
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var ddl = (DropDownList)crates.FindByName(name);
            ddl.SelectByKey(ddl.ListItems.Where(l => l.Key.Contains(key)).FirstOrDefault()?.Key);
        }

        private void SetSprint(ActivityPayload payload)
        {
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var sprints = (DropDownList)crates.FindByName("Sprint");
            var sprint = sprints.ListItems.FirstOrDefault();
            if(sprint != null)
            {
                sprints.SelectByKey(sprint.Key);
            }
        }

        private void SetMessage(ActivityPayload payload, string name, string body)
        {
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var nameTextBox = (TextBox)crates.FindByName("Name");
            var bodyTextBox = (BuildMessageAppender)crates.FindByName("Body");
            nameTextBox.Value = name;
            bodyTextBox.Value = body;
        }

        private ActivityTemplateDTO GetActivityTemplate(IEnumerable<ActivityTemplateDTO> activityList, string activityTemplateName, string version = null)
        {
            var template = activityList.FirstOrDefault(x => {
                if (!string.IsNullOrEmpty(version))
                {
                    return x.Name == activityTemplateName && x.Version == version;
                }
                return x.Name == activityTemplateName;
            });
            if (template == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            return template;
        }
    }
}