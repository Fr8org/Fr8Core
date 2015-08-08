using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class PluginDO : BaseDO, IPluginRegistrationDO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("PluginStatusTemplate")]
        public int PluginStatus { get; set; }
        public _PluginStatusTemplate PluginStatusTemplate { get; set; }
    }
}
