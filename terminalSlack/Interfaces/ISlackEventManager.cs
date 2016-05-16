using System;
using System.Threading.Tasks;
using Data.Entities;
using TerminalBase.Models;

namespace terminalSlack.Interfaces
{
    public interface ISlackEventManager : IDisposable
    {
        Task Subscribe(AuthorizationToken token, Guid planId);

        void Unsubscribe(Guid planId);
    }
}
