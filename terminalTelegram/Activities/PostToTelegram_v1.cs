using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalTelegram.TelegramIntegration;

namespace terminalTelegram.Activities
{
    public class PostToTelegram_v1 : TerminalActivity<PostToTelegram_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Publish_To_Telegram",
            Label = "Publish To Telegram",
            Tags = "Notifier",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            Version = "1",
            WebService = TerminalData.WebServiceDTO,
            MinPaneWidth = 330,
            NeedsAuthentication = false
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox PhoneNumber { get; set; }

            public TextBox Code { get; set; }

            public TextBox Message { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                //MessageSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Message", nameof(MessageSource), requestUpstream: true, availability: AvailabilityType.RunTime);
                Controls.Add(PhoneNumber = new TextBox { Label = "Phone Number" });
                Controls.Add(Code = new TextBox { Label = "Code" });
                Controls.Add(Message = new TextBox { Label = "Message" });
            }
        }

        private readonly ITelegramIntegration _telegramIntegration;

        public PostToTelegram_v1(ICrateManager crateManager, ITelegramIntegration telegramIntegration)
            : base(crateManager)
        {
            _telegramIntegration = telegramIntegration;
            DisableValidationOnFollowup = true;
        }

        public override async Task Initialize()
        {
            //No extra config is required
            await Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            //No extra config is required
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            //ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.Message.Value, "Can't post empty message to Telegram");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var result = _telegramIntegration.ConnectAsync();
            var phoneNumber = ActivityUI.PhoneNumber.Value;
            var code = ActivityUI.Code.Value;
            var message = ActivityUI.Message.Value;
            var hashResult = _telegramIntegration.GetHashAsync(phoneNumber);
            var hash = await hashResult;
            var res1 = _telegramIntegration.MakeAuthAsync(phoneNumber, hash, code);
            var userId = await _telegramIntegration.GetUserIdAsync(phoneNumber);
            await _telegramIntegration.PostMessageToUserAsync(userId.Value, message).ConfigureAwait(false);
            //if (!success)
            //{
            //    throw new ActivityExecutionException("Failed to post message to Telegram");
            //}
        }
    }
}