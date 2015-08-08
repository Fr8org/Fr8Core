using System;

namespace Data.Exceptions
{
    public class EntityNotFoundException : ApplicationException
    {
        public object Id { get; private set; }

        public EntityNotFoundException()
        {
            
        }

        public EntityNotFoundException(object id)
        {
            Id = id;
        }

        public EntityNotFoundException(object id, string message)
            : base(message)
        {
            Id = id;
        }
    }

    public class EntityNotFoundException<T> : EntityNotFoundException
    {
        public EntityNotFoundException()
            : base(null, string.Format("{0} not found.", typeof(T).Name))
        {
            
        }

        public EntityNotFoundException(object id)
            : base(id, string.Format("{0} #{1} not found.", typeof(T).Name, id))
        {
        }

        public EntityNotFoundException(object id, string message)
            : base(id, message)
        {
        }
    }
}
