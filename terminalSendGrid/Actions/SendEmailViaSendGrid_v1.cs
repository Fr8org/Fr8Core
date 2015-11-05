using System.Collections.Generic;
using System.Threading.Tasks;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Enums;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Utilities;
using terminalSendGrid.Infrastructure;

namespace terminalSendGrid.Actions
{
    public class SendEmailViaSendGrid_v1 : BasePluginAction
    {
        // moved the EmailPackager ObjectFactory here since the basepluginAction will be called by others and the dependency is defiend in pluginsendGrid
        private IConfigRepository _configRepository;
        private IEmailPackager _emailPackager;
        

        // protected override void SetupServices()
        // {
        //     base.SetupServices();
        //     _emailPackager = ObjectFactory.GetInstance<IEmailPackager>();
        // }

        public SendEmailViaSendGrid_v1()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _emailPackager = ObjectFactory.GetInstance<IEmailPackager>();
        }

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDO, EvaluateReceivedRequest, authTokenDO);
        }

        /// <summary>
        /// this entire function gets passed as a delegate to the main processing code in the base class
        /// currently many actions have two stages of configuration, and this method determines which stage should be applied
        /// </summary>
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDO curActionDO)
        {
            var curCrates = curActionDO.CrateStorageDTO();

            if (curCrates == null || curCrates.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.CrateStorageDTO() == null)
            {
                curActionDO.UpdateCrateStorageDTO(new CrateStorageDTO().CrateDTO);
            }

            curActionDO.CrateStorageDTO().CrateDTO.Add(CreateControlsCrate());
            curActionDO.CrateStorageDTO().CrateDTO.Add(await GetAvailableDataFields(curActionDO));

            return curActionDO;
        }

        /// <summary>
        /// Create EmailAddress RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailAddressRadioButtonGroup()
        {
            var control = CreateSpecificOrUpstreamValueChooser(
                "For the Email Address, use",
                "EmailAddress",
                "Upstream Plugin-Provided Fields"
            );

            return control;
        }

        /// <summary>
        /// Create EmailSubject RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailSubjectRadioButtonGroup()
        {
            var control = CreateSpecificOrUpstreamValueChooser(
                "For the Email Subject, use",
                "EmailSubject",
                "Upstream Plugin-Provided Fields"
            );

            return control;
        }

        /// <summary>
        /// Create EmailBody RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailBodyRadioButtonGroup()
        {
            var control = CreateSpecificOrUpstreamValueChooser(
                "For the Email Body, use",
                "EmailBody",
                "Upstream Plugin-Provided Fields"
            );

            return control;
        }

        private CrateDTO CreateControlsCrate()
        {
            var controls = new[]
            {
                CreateEmailAddressRadioButtonGroup(),
                CreateEmailSubjectRadioButtonGroup(),
                CreateEmailBodyRadioButtonGroup()
            };

            return Crate.CreateStandardConfigurationControlsCrate("Send Grid", controls);
        }

        private async Task<CrateDTO> GetAvailableDataFields(ActionDO curActionDO)
        {
            var curUpstreamFields =
                (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Upstream))
                    .Fields
                    .ToArray();

            var crateDTO = Crate.CreateDesignTimeFieldsCrate(
                "Upstream Plugin-Provided Fields",
                curUpstreamFields
            );

            return crateDTO;
        }

        public object Activate(ActionDO curActionDO)
        {
            //not currently any requirements that need attention at Activation Time
            return curActionDO;
        }

        public object Deactivate(ActionDO curActionDO)
        {
            return curActionDO;
        }

        private string CreateEmailHTMLText(string emailBody)
        {
            var template = @"<html><body>{0}</body></html>";
            var htmlText = string.Format(template, emailBody);

            return htmlText;
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            var fromAddress = _configRepository.Get("OutboundFromAddress");

            var processPayload = await GetProcessPayload(containerId);

            var emailAddress = ExtractSpecificOrUpstreamValue(
                curActionDO.CrateStorageDTO(),
                processPayload.CrateStorageDTO(),
                "EmailAddress"
            );
            var emailSubject = ExtractSpecificOrUpstreamValue(
                curActionDO.CrateStorageDTO(),
                processPayload.CrateStorageDTO(),
                "EmailSubject"
            );
            var emailBody = ExtractSpecificOrUpstreamValue(
                curActionDO.CrateStorageDTO(),
                processPayload.CrateStorageDTO(),
                "EmailBody"
            );

            var mailerDO = new PluginMailerDO()
            {
                Email = new EmailDO()
                {
                    From = new EmailAddressDO
                    {
                        Address = fromAddress,
                        Name = "Fr8 Operations"
                    },

                    Recipients = new List<RecipientDO>()
                    {
                        new RecipientDO()
                        {
                            EmailAddress = new EmailAddressDO(emailAddress),
                            EmailParticipantType = EmailParticipantType.To
                        }
                    },
                    Subject = emailSubject,
                    HTMLText = CreateEmailHTMLText(emailBody)
                }
            };

            await _emailPackager.Send(mailerDO);

            return processPayload;
        }
    }
}