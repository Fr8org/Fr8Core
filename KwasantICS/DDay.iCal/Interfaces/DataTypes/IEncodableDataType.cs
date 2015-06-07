namespace KwasantICS.DDay.iCal.Interfaces.DataTypes
{
    public interface IEncodableDataType :
        ICalendarDataType
    {
        string Encoding { get; set; }
    }
}
