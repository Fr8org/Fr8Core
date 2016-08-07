using System;

namespace Hub.Exceptions
{
    public class ManifestGenerationException : Exception
    {
        public ManifestGenerationException(string message) : base(message) { }
    }
}