using System.Threading.Tasks;
using TLSharp.Core;
using TLSharp.Core.MTProto;

namespace terminalTelegram.TelegramIntegration
{
    public class TelegramIntegration : ITelegramIntegration
    {
        public TelegramClient TelegramClient { get; set; }

        private const int ApiId = 49052;
        private const string ApiHash = "fedb7f04dc908a91ddf80bb7fb20d79c";

        public TelegramIntegration()
        {
        }

        public async Task ConnectAsync()
        {
            var store = new FileSessionStore();
            TelegramClient = new TelegramClient(store, "session", ApiId, ApiHash);
            await TelegramClient.Connect();
        }

        public async Task<string> GetHashAsync(string phoneNumber)
        {
            return await TelegramClient.SendCodeRequest(phoneNumber);
        }

        public async Task<User> MakeAuthAsync(string phoneNumber, string hash, string code)
        {
            return await TelegramClient.MakeAuth(phoneNumber, hash, code);
        }

        public async Task<int?> GetUserIdAsync(string phoneNumber)
        {
            return await TelegramClient.ImportContactByPhoneNumber(phoneNumber);
        }

        public async Task PostMessageToUserAsync(int userId, string message)
        {
            await TelegramClient.SendMessage(userId, message);
        }
    }
}