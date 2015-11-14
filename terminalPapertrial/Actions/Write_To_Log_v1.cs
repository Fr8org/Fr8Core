
using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using System.Collections.Generic;
using Utilities.Logging;

namespace terminalPapertrial.Actions
{
    /// <summary>
    /// Write To Log action which writes Log Messages to Papertrial at run time
    /// </summary>
    public class Write_To_Log_v1 : BaseTerminalAction
    {
        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            var targetUrlTextBlock = new TextBoxControlDefinitionDTO
            {
                Name = "TargetUrlTextBox",
                Label = "Target Papertrial URL (As URL:PORT format)",
                Value = CloudConfigurationManager.GetSetting("PapertrialDefaultLogUrl"),
                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
            };

            var curControlsCrate = PackControlsCrate(targetUrlTextBlock);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = new CrateStorage(curControlsCrate);
            }

            return await Task.FromResult(curActionDO);
        }

        public Task<object> Activate(ActionDO curActionDO)
        {
            //Responsibility is not yet defined.

            return Task.FromResult((object) true);
        }

        public object Deactivate(ActionDO curDataPackage)
        {
            //Responsibility is not yet defined.

            return true;
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            //get the paper trial URL value fromt configuration control
            string curPapertrialUrl;
            int curPapertrialPort;

            GetPapertrialTargetUrlAndPort(actionDO, out curPapertrialUrl, out curPapertrialPort);

            //get process payload
            var curProcessPayload = await GetProcessPayload(containerId);

            //if there are valid URL and Port number
            if (!string.IsNullOrEmpty(curPapertrialUrl) && curPapertrialPort > 0)
            {
                //get log message
                var curLogMessages = Crate.GetStorage(curProcessPayload).CrateContentsOfType<StandardLoggingCM>().First();

                curLogMessages.Item.ForEach(logMessage =>
                {
                    var papertrialLogger = Logger.GetPapertrialLogger(curPapertrialUrl, curPapertrialPort);
                    papertrialLogger.Info(logMessage.Data);
                });
            }

            return curProcessPayload;
        }

        private void GetPapertrialTargetUrlAndPort(ActionDO curActionDO, out string paperrrialTargetUrl, out int papertrialTargetPort)
        {
            //get the configuration control of the given action
            var curActionConfigControls =
                Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            //the URL is given in "URL:PortNumber" format. Parse the input value to get the URL and port number
            var targetUrlValue = curActionConfigControls.FindByName("TargetUrlTextBox").Value.Split(new char[] { ':' });

            //assgign the output value
            paperrrialTargetUrl = targetUrlValue[0];
            papertrialTargetPort = Convert.ToInt32(targetUrlValue[1]);
        }
    }
}