using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IAttachmentDO : IBaseDO
    {
        [Key]
        int Id { get; set; }
    }
}