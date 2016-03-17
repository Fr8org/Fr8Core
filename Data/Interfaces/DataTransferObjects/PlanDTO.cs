using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces.DataTransferObjects
{
    public class PlanDTO
    {
        public PlanFullDTO Plan { get; set; }
    }
}
