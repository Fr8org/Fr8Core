namespace Data.Interfaces
{
    public interface ICustomField
    {
        int Id { get; set; }
        string ForeignTableName { get; set; }
    }
}
