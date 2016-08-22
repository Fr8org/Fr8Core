using System;

namespace Hub.Exceptions
{
    /// <summary>
    /// A base class for Fr8 messages which support both a message for internal logging and a message to User.
    /// </summary>
    public class Fr8Exception : Exception
    {
        public string UserMessage;
        public string UserId;

        /// <summary>
        /// Creates a new Fr8 exception.
        /// </summary>
        public Fr8Exception()
        { }

        /// <summary>
        /// Creates a new Fr8 exception with a message for internal logging.
        /// </summary>
        /// <param name="message">Message for internal logging</param>
        public Fr8Exception(string message) : base(message)
        { }

        /// <summary>
        /// Creates a new Fr8 exception with a message for internal logging and a separate message to User. 
        /// </summary>
        /// <param name="message">Message for internal logging.</param>
        /// <param name="userMessage">Message to User (displayed in the UI).</param>
        public Fr8Exception(string message, string userMessage) : base(message)
        {
            UserMessage = userMessage;
        }
    }

    public class Fr8ConflictException : Fr8Exception
    {
        public Fr8ConflictException(string typeName, string identityName, string identityValue)
            : base($"The object of type {typeName} with '{identityName}' equal to '{identityValue}' already exists.") { }
    }

    public class Fr8ArgumentException : Fr8Exception
    {
        public string ArgumentName;

        public Fr8ArgumentException(string argumentName)
            : base($"Argument {argumentName} is not valid.")
        {
            ArgumentName = argumentName;
        }

        public Fr8ArgumentException(string argumentName, string userMessage)
            : base($"Argument {argumentName} is not valid.", userMessage)
        {
            ArgumentName = argumentName;
        }

        public Fr8ArgumentException(string argumentName, string message, string userMessage)
            : base(message, userMessage)
        {
            ArgumentName = argumentName;
        }
    }

    public class Fr8ArgumentNullException : Fr8Exception
    {
        public string ArgumentName;

        public Fr8ArgumentNullException(string argumentName)
            : base($"Argument {argumentName} is null.")
        {
            ArgumentName = argumentName;
        }

        public Fr8ArgumentNullException(string argumentName, string userMessage)
            : base($"Argument {argumentName} is null.", userMessage)
        {
            ArgumentName = argumentName;
        }

        public Fr8ArgumentNullException(string argumentName, string message, string userMessage)
            : base(message, userMessage)
        {
            ArgumentName = argumentName;
        }
    }

    public class Fr8ArgumentOutOfRangeException : Fr8Exception
    {
        public string ArgumentName;

        public Fr8ArgumentOutOfRangeException(string argumentName)
            : base($"Argument {argumentName} is out of range of acceptable values.")
        {
            ArgumentName = argumentName;
        }

        public Fr8ArgumentOutOfRangeException(string argumentName, string userMessage)
            : base($"Argument {argumentName} is out of range of acceptable values.", userMessage)
        {
            ArgumentName = argumentName;
        }

        public Fr8ArgumentOutOfRangeException(string argumentName, string message, string userMessage)
            : base(message, userMessage)
        {
            ArgumentName = argumentName;
        }
    }

    public class Fr8InvalidOperationException : Fr8Exception
    {
        public string ArgumentName;

        public Fr8InvalidOperationException(string message)
            : base(message)
        {
        }

        public Fr8InvalidOperationException(string message, string userMessage)
            : base(message, userMessage)
        {
        }
    }

    public class Fr8InsifficientPermissionsException : Fr8Exception
    {
        public string ArgumentName;

        public Fr8InsifficientPermissionsException(string message)
            : base(message)
        {
        }

        public Fr8InsifficientPermissionsException(string message, string userMessage)
            : base(message, userMessage)
        {
        }
    }

    public class Fr8NotFoundException : Fr8Exception
    {
        public string ArgumentName;

        public Fr8NotFoundException(string message)
            : base(message)
        {
        }

        public Fr8NotFoundException(string message, string userMessage)
            : base(message, userMessage)
        {
        }
    }
}
