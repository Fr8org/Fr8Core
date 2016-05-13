using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using StructureMap;
using terminalPapertrail.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;

namespace terminalPapertrail.Actions
{
    /// <summary>
    /// Write To Log action which writes Log Messages to Papertrail at run time
    /// </summary>
    public class Write_To_Log_v1 : BaseTerminalActivity
    {
        private IPapertrailLogger _papertrailLogger;

        public Write_To_Log_v1()
        {
            _papertrailLogger = ObjectFactory.GetInstance<IPapertrailLogger>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO = null)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO = null)
        {
            var targetUrlTextBlock = new TextBox
            {
                Name = "TargetUrlTextBox",
                Label = "Target Papertrail URL and Port (as URL:Port)",
                Value = CloudConfigurationManager.GetSetting("PapertrailDefaultLogUrl"),
                Required = true
            };

            var curControlsCrate = PackControlsCrate(targetUrlTextBlock);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(new CrateStorage(curControlsCrate));
            }

            return await Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            //get process payload
            var curProcessPayload = await GetPayload(activityDO, containerId);

            //get the Papertrail URL value fromt configuration control
            string curPapertrailUrl;
            int curPapertrailPort;

            try
            {
                GetPapertrailTargetUrlAndPort(activityDO, out curPapertrailUrl, out curPapertrailPort);
            }
            catch (ArgumentException e)
            {
                return Error(curProcessPayload, e.Message);
            }

            //if there are valid URL and Port number
            if (!string.IsNullOrEmpty(curPapertrailUrl) && curPapertrailPort > 0)
            {
                //get log message
                var curLogMessages = CrateManager.GetStorage(curProcessPayload).CrateContentsOfType<StandardLoggingCM>().Single();

                curLogMessages.Item.Where(logMessage => !logMessage.IsLogged).ToList().ForEach(logMessage =>
                {
                    _papertrailLogger.LogToPapertrail(curPapertrailUrl, curPapertrailPort, logMessage.Data);
                    logMessage.IsLogged = true;
                });

                using (var crateStorage = CrateManager.GetUpdatableStorage(curProcessPayload))
                {
                    crateStorage.RemoveByLabel("Log Messages");
                    crateStorage.Add(Fr8Data.Crates.Crate.FromContent("Log Messages", curLogMessages));
                }
            }

            return Success(curProcessPayload);
        }

        private void GetPapertrailTargetUrlAndPort(ActivityDO curActivityDO, out string papertrialTargetUrl, out int papertrailTargetPort)
        {
            //get the configuration control of the given action
            var curActionConfigControls =
                CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            //the URL is given in "URL:PortNumber" format. Parse the input value to get the URL and port number
            var targetUrlValue = curActionConfigControls.FindByName("TargetUrlTextBox").Value.Split(new char[] { ':' });

            if (targetUrlValue.Length != 2)
            {
                throw new ArgumentException("Papertrail URL and PORT are not in the correct format. The given URL is " +
                                            curActionConfigControls.FindByName("TargetUrlTextBox").Value);
            }

            //assgign the output value
            papertrialTargetUrl = targetUrlValue[0];
            papertrailTargetPort = Convert.ToInt32(targetUrlValue[1]);
        }
    }
}