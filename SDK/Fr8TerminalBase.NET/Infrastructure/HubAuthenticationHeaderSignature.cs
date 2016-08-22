using System;
using System.Collections.Generic;
using System.Net.Http;
using Fr8.Infrastructure.Interfaces;

namespace Fr8.TerminalBase.Infrastructure
{
    public class HubAuthenticationHeaderSignature : IRequestSignature
    {
        private readonly string _fr8Token;
        public HubAuthenticationHeaderSignature(string token, string userId)
        {
            _fr8Token = $"key={token}" + (string.IsNullOrEmpty(userId) ? "" : $", user={userId}");
        }

        public void SignRequest(HttpRequestMessage request)
        {
            request.Headers.Add(System.Net.HttpRequestHeader.Authorization.ToString(), $"FR8-TOKEN {_fr8Token}");
        }
    }
}