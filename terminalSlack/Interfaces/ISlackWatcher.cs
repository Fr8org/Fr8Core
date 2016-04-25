using System;
using SlackAPI;

namespace terminalSlack.Interfaces
{
    public interface ISlackWatcher
    {
        event Action<Message> MessageRecieved;
    }
}