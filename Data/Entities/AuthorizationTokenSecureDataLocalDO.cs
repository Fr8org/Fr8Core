using System;

namespace Data.Entities
{
    public class AuthorizationTokenSecureDataLocalDO : BaseDO
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
