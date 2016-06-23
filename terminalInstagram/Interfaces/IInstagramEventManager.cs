using System;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;

namespace terminalInstagram.Interfaces
{
    public interface IInstagramEventManager : IDisposable
    {
        Task Subscribe(AuthorizationToken token, Guid planId);
        void Unsubscribe(Guid planId);
    }
}
