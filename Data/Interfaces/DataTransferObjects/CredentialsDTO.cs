using System;

namespace Data.Interfaces.DataTransferObjects
{
    public class CredentialsDTO
    {
        public int TerminalId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
}
