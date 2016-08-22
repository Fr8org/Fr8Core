namespace Data.Interfaces
{
    public interface IInvitationResponseDO : IEmailDO
    {
        int? AttendeeId { get; set; }
    }
}
