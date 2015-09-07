using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
	public class ActivityDO : BaseDO, IActionListChild
    {
        [Key]
        public int Id { get; set; }

        public int Ordering { get; set; }

		[ForeignKey("ParentActionList")]
		  public int? ParentActionListId { get; set; }

		  public virtual ActionListDO ParentActionList { get; set; }
	 }
}