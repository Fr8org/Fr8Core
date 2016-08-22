namespace Hub.Interfaces
{
    public interface ISMSMessage
    {
        /// <summary>
        /// Sends SMS message to the given number
        /// </summary>
        /// <param name="number">Phoen Number to send SMS</param>
        /// <param name="message">SMS Message Text</param>
        void Send(string number, string message);
    }
}
