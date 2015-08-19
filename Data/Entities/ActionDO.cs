using Data.States.Templates;
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

        /// <summary>
        /// This property defines the permanent postion of Actions in ActionList.
        /// </summary>
        public int Ordering  { get; set; }

        [ForeignKey("ActionStateTemplate")]
        public string ActionState { get; set; }

        public virtual _ActionStateTemplate ActionStateTemplate { get; set; }
	}
}