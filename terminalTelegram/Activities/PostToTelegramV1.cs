using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Services;
using terminalTelegram.TelegramIntegration;
using System;

namespace terminalTelegram.Activities
{
    public class PostToTelegramV1 : TerminalActivity<PostToTelegramV1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("08252B74-F829-48D5-B25E-69F97604BF67"),
            Name = "Post_To_Telegram",
            Label = "Post To Telegram",
            Tags = "Notifier",
            Terminal = TerminalData.TerminalDTO,
            Version = "1",
            MinPaneWidth = 330,
            NeedsAuthentication = true,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox PhoneNumber { get; set; }

            public TextSource MessageSource { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                MessageSource = uiBuilder.CreateSpecificOrUpstreamValueChooser(
                    "Message", 
                    nameof(MessageSource), 
                    requestUpstream: true, 
                    availability: AvailabilityType.RunTime);

                Controls.Add(PhoneNumber = new TextBox { Label = "Phone Number" });
                Controls.Add(MessageSource);
            }
        }

        private readonly ITelegramIntegration _telegramIntegration;

        public PostToTelegramV1(ICrateManager crateManager, ITelegramIntegration telegramIntegration)
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
            ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.MessageSource, "Can't post empty message to Telegram");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            // Phone to send message
            var phoneNumber = ActivityUI.PhoneNumber.Value;
            // Message
            var message = ActivityUI.MessageSource.TextValue;

            await _telegramIntegration.ConnectAsync();

            // Gets userId from phone number
            var userId = await _telegramIntegration.GetUserIdAsync(phoneNumber);
            // Send message to user
            await _telegramIntegration.PostMessageToUserAsync(userId.Value, message).ConfigureAwait(false);
        }
    }
}