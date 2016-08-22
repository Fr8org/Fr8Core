using System;

namespace Fr8Data.DataTransferObjects
{
    public class CredentialsDTO
    {
        public TerminalDTO Terminal { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public bool IsDemoAccount { get; set;  }
        public string Fr8UserId { get; set; }
    }
}
