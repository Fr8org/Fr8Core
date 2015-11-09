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
    public class PluginDO : BaseDO, IPluginDO
    {
        public PluginDO()
        {

        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }

        [ForeignKey("PluginStatusTemplate")]
        public int PluginStatus { get; set; }
        public _PluginStatusTemplate PluginStatusTemplate { get; set; }

        // TODO: remove this, DO-1397
        // public bool RequiresAuthentication { get; set; }

        //public string BaseEndPoint { get; set; }

        public string Endpoint { get; set; }
    }
}
