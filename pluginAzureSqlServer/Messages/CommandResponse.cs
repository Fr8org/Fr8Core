namespace pluginAzureSqlServer.Messages
{
    public class CommandResponse
    {
        public static CommandResponse SuccessResponse(object data)
        {
            return new CommandResponse(true, data, null);
        }

        public static CommandResponse ErrorResponse(string message)
        {
            return new CommandResponse(false, null, message);
        }


        internal CommandResponse(bool success, object data, string message)
        {
            _success = success;
            _data = data;
            _message = message;
        }

        public bool Success { get { return _success; } }

        public object Data { get { return _data; } }

        public string Message { get { return _message; } }


        private readonly bool _success;
        private readonly object _data;
        private readonly string _message;
    }
}