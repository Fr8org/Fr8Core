using System;
using System.Collections.Generic;
using System.Net.Http;
using Fr8.Infrastructure.Interfaces;

namespace Fr8.TerminalBase.Infrastructure
{
    public class HubAuthenticationHeaderSignature : IRequestSignature
    {
        private readonly string _fr8Token;
        public HubAuthenticationHeaderSignature(string token)
        {
            _fr8Token = $"FR8 terminal_key={token}";
        }

        public void SignRequest(HttpRequestMessage request)
        {
            request.Headers.Add(System.Net.HttpRequestHeader.Authorization.ToString(), _fr8Token);
        }
    }
}