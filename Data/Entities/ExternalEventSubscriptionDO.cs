using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
	public class ExternalEventSubscriptionDO
	{
		[ Key ]
		public int Id{ get; set; }

		public int EventType{ get; set; }

		[ ForeignKey( "ProcessTemplate" ) ]
		public int ProcessTemplateId{ get; set; }

		public virtual ProcessTemplateDO ProcessTemplate{ get; set; }
	}
}