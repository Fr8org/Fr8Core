using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
	public class WebServiceDO : BaseObject
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public string IconPath { get; set; }
	}
}