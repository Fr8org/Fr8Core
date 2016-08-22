using System.Threading.Tasks;
using TLSharp.Core;
using TLSharp.Core.MTProto;

namespace terminalTelegram.TelegramIntegration
{
    public interface ITelegramIntegration
    {
        TelegramClient TelegramClient { get; set; }

        Task ConnectAsync();

        Task<string> GetHashAsync(string phoneNumber);

        Task<User> MakeAuthAsync(string phoneNumber, string hash, string code);

        Task<int?> GetUserIdAsync(string phoneNumber);

        Task PostMessageToUserAsync(int userId, string message);
    }
}