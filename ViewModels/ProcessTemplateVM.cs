using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Data.States;

namespace Web.ViewModels
{
    public class ProcessTemplateVM
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProcessTemplateState { get; set; }
    }
}