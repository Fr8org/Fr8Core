using System;

namespace PlanDirectory.Exceptions
{
    public class ManifestGenerationException : Exception
    {
        public ManifestGenerationException(string message) : base(message) { }
    }
}