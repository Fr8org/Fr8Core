using System;

namespace Data.Entities
{
    public class EncryptedAuthorizationData 
    {
        public Guid Id
        {
            get; 
            set;
        }

        public string Data
        {
            get;
            set;
        }
    }
}
