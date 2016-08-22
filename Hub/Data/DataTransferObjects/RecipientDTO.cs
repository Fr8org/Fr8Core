using System;

namespace Fr8Data.DataTransferObjects
{
    public class RecipientDTO
    {
        public int Id { get; set; }
        public int? EmailID { get; set; }
        public virtual EmailDTO Email { get; set; }
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDTO EmailAddress { get; set; }
        public int? EmailParticipantType { get; set; }
    }
}
