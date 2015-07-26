using System;
using System.Collections.Generic;
using Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface ICalendarDO : IBaseDO
    {
        [Key]
        int Id { get; set; }

        String Name { get; set; }

        DockyardAccountDO Owner { get; set; }

        List<EventDO> Events { get; set; }
    }
}