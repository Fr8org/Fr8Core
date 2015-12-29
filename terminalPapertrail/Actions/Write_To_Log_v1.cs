
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalPapertrail.Interfaces;
using Utilities.Configuration.Azure;
using System.Collections.Generic;
using Data.Control;
using Utilities.Logging;

namespace terminalPapertrail.Actions
{
    /// <summary>
    /// Write To Log action which writes Log Messages to Papertrail at run time
    /// </summary>
    public class Write_To_Log_v1 : BaseTerminalAction
    {
        private IPapertrailLogger _papertrailLogger;

        public Write_To_Log_v1()
        {
            _papertrailLogger = ObjectFactory.GetInstance<IPapertrailLogger>();
        }

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
            var targetUrlTextBlock = new TextBox
            {
                Name = "TargetUrlTextBox",
                Label = "Target Papertrail URL and Port (as URL:Port)",
                Value = CloudConfigurationManager.GetSetting("PapertrailDefaultLogUrl"),
                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")},
                Required = true
            };

            var curControlsCrate = PackControlsCrate(targetUrlTextBlock);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = new CrateStorage(curControlsCrate);
            }

            return await Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            //get process payload
            var curProcessPayload = await GetPayload(actionDO, containerId);

            //get the Papertrail URL value fromt configuration control
            string curPapertrailUrl;
            int curPapertrailPort;

            try
            {
                GetPapertrailTargetUrlAndPort(actionDO, out curPapertrailUrl, out curPapertrailPort);
            }
            catch (ArgumentException e)
            {
                return Error(curProcessPayload, e.Message);
            }

            //if there are valid URL and Port number
            if (!string.IsNullOrEmpty(curPapertrailUrl) && curPapertrailPort > 0)
            {
                //get log message
                var curLogMessages = Crate.GetStorage(curProcessPayload).CrateContentsOfType<StandardLoggingCM>().Single();

                curLogMessages.Item.Where(logMessage => !logMessage.IsLogged).ToList().ForEach(logMessage =>
                {
                    _papertrailLogger.LogToPapertrail(curPapertrailUrl, curPapertrailPort, logMessage.Data);
                    logMessage.IsLogged = true;
                });

                using (var updater = Crate.UpdateStorage(curProcessPayload))
                {
                    updater.CrateStorage.RemoveByLabel("Log Messages");
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Log Messages", curLogMessages));
                }
            }

            return Success(curProcessPayload);
        }

        private void GetPapertrailTargetUrlAndPort(ActionDO curActionDO, out string paperrrialTargetUrl, out int papertrailTargetPort)
        {
            //get the configuration control of the given action
            var curActionConfigControls =
                Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            //the URL is given in "URL:PortNumber" format. Parse the input value to get the URL and port number
            var targetUrlValue = curActionConfigControls.FindByName("TargetUrlTextBox").Value.Split(new char[] { ':' });

            if (targetUrlValue.Length != 2)
            {
                throw new ArgumentException("Papertrail URL and PORT are not in the correct format. The given URL is " +
                                            curActionConfigControls.FindByName("TargetUrlTextBox").Value);
            }

            //assgign the output value
            paperrrialTargetUrl = targetUrlValue[0];
            papertrailTargetPort = Convert.ToInt32(targetUrlValue[1]);
        }
    }
}