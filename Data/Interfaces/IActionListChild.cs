using Data.Entities;

namespace Data.Interfaces
{
    public interface IActionListChild
    {
        int? ParentActionListId { get; set; }
        ActionListDO ParentActionList { get; set; }
    }
}
