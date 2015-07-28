namespace Core.Interfaces
{
    public interface IProcessService
    {
        void HandleDocusignNotification(string userId, string xmlPayload);
    }
}
