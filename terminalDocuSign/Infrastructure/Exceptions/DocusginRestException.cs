using System;
using DocuSign.Integrations.Client;

namespace terminalDocuSign.Infrastructure
{
    public class DocusginRestException : Exception
    {
        public readonly Error Error;

        public DocusginRestException(Error error)
        {
            Error = error;
        }
    }
}