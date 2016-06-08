using System;
using System.Linq;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Control;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;
using terminalPapertrail.Interfaces;

namespace terminalPapertrail.Actions
{
    /// <summary>
    /// Write To Log action which writes Log Messages to Papertrail at run time
    /// </summary>
    public class Write_To_Log_v1 : BaseTerminalActivity
    {
        private IPapertrailLogger _papertrailLogger;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Write_To_Log",
            Label = "Write To Log",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Write_To_Log_v1(ICrateManager crateManager)
            : base(crateManager)
        {
            _papertrailLogger = ObjectFactory.GetInstance<IPapertrailLogger>();
        }

        public override async Task Initialize()
        {
            var targetUrlTextBlock = new TextBox
            {
                Name = "TargetUrlTextBox",
                Label = "Target Papertrail URL and Port (as URL:Port)",
                Value = CloudConfigurationManager.GetSetting("PapertrailDefaultLogUrl"),
                Required = true
            };

            var curControlsCrate = PackControlsCrate(targetUrlTextBlock);

            Storage.Add(curControlsCrate);
        }

        public override async Task Run()
        {
            //get the Papertrail URL value fromt configuration control
            string curPapertrailUrl;
            int curPapertrailPort;

            try
            {
                GetPapertrailTargetUrlAndPort(out curPapertrailUrl, out curPapertrailPort);
            }
            catch (ArgumentException e)
            {
                RaiseError(e.Message);
                return;
            }

            //if there are valid URL and Port number
            if (!string.IsNullOrEmpty(curPapertrailUrl) && curPapertrailPort > 0)
            {
                //get log message
                var curLogMessages = Payload.CrateContentsOfType<StandardLoggingCM>().Single();

                curLogMessages.Item.Where(logMessage => !logMessage.IsLogged).ToList().ForEach(logMessage =>
                {
                    _papertrailLogger.LogToPapertrail(curPapertrailUrl, curPapertrailPort, logMessage.Data);
                    logMessage.IsLogged = true;
                });

                Payload.RemoveByLabel("Log Messages");
                Payload.Add(Crate.FromContent("Log Messages", curLogMessages));
             }
             Success();
        }



        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        private void GetPapertrailTargetUrlAndPort(out string papertrialTargetUrl, out int papertrailTargetPort)
        {
            //get the configuration control of the given action
            var curActionConfigControls =
                Storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

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