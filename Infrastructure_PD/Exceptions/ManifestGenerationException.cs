using System;

namespace HubWeb.Infrastructure_PD.Exceptions
{
    public class ManifestGenerationException : Exception
    {
        public ManifestGenerationException(string message) : base(message) { }
    }
}