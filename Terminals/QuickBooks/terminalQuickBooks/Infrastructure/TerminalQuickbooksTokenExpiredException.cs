using System;

namespace terminalQuickBooks.Infrastructure
{
    public class TerminalQuickbooksTokenExpiredException : Exception
    {
        public TerminalQuickbooksTokenExpiredException(string message) : base(message)
        {
        }
    }
}