using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
	public interface IEnvelopeDO: IBaseDO
	{
		[ Key ]
		int Id{ get; set; }
	}
}