namespace Shnexy
{
    public interface IDaemon
    {
        int WaitTimeBetweenExecution { get; }
        void Start();
        void Stop();
    }
}
