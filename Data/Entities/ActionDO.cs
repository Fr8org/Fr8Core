using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
	public class ActionDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		public string Name{ get; set; }

		[ ForeignKey( "ActionList" ) ]
		public int? ActionListID{ get; set; }
		public virtual ActionListDO ActionList{ get; set; }
	}
}