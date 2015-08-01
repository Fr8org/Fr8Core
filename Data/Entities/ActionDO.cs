using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
	public class ActionDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		public string Name{ get; set; }

        public string ConfigurationSettings { get; set; }

        public string FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }

		[ ForeignKey( "ActionList" ) ]
		public int? ActionListID{ get; set; }
		public virtual ActionListDO ActionList{ get; set; }
	}
}