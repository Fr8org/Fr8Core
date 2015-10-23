using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
	public class WebServiceDO : BaseDO
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
	}
}