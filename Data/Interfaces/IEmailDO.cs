using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IEmailDO : IBaseDO
    {
        [Key]
        int Id { get; set; }

        EmailAddressDO From { get; set; }
        String Subject { get; set; }
        String HTMLText { get; set; }
        DateTimeOffset DateReceived { get; set; }

        IEnumerable<EmailAddressDO> To { get; }
       
        //List<EventDO> Events { get; set; }
        int? EmailStatus { get; set; }
    }
}