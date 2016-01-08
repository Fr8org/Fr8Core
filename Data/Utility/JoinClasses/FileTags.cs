using Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data.Utility.JoinClasses
{
    public class FileTags : BaseObject
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Tag")]
        public int TagId { get; set; }
        public virtual TagDO Tag { get; set; }

        [ForeignKey("File")]
        public int FileDoId { get; set; }
        public virtual FileDO File { get; set; }
    }
}
