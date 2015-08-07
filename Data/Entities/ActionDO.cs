using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
	public class ActionDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

        public string UserLabel{ get; set; }

        public string ActionType { get; set; }

        [ForeignKey("ActionList")]
        public int? ActionListId { get; set; }
        public virtual ActionListDO ActionList { get; set; }

        public string ConfigurationSettings { get; set; }

        public string FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }

        public int Ordering  { get; set; }
	}
}