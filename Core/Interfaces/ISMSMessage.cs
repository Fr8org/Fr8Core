namespace Core.Interfaces
{
    public interface ISMSMessage
    {
        void Send(string number, string message);
    }
}
