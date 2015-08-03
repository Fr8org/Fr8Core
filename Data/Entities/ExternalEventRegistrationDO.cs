using System.ComponentModel.DataAnnotations.Schema;
using Data.States;

namespace Data.Entities
{
	public class ExternalEventRegistrationDO
	{
		public ExternalEventType EventType{ get; set; }

		[ ForeignKey( "ProcessTemplateDO" ) ]
		public int? ProcessTemplateId{ get; set; }

		public virtual ProcessTemplateDO ProcessTemplate{ get; set; }
	}
}