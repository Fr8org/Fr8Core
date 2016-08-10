using System;

namespace Fr8.Infrastructure
{
    /// <summary>
    /// Represents errors that occur if specific object is missing in the storage
    /// </summary>
    public class MissingObjectException : Exception
    {
        public MissingObjectException(string message)
            : base(message)
        {
        }
    }
}
