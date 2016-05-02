using System;

namespace terminalSlack.RtmClient
{
    public class DataEventArgs<TData> : EventArgs
    {
        public TData Data { get; private set; }

        public DataEventArgs(TData data)
        {
            Data = data;
        }
    }
}