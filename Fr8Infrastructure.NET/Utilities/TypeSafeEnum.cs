using System;
using System.Collections.Generic;
using System.Linq;

namespace Shnexy.Utilities
{


    // E is the derived type-safe-enum class
    // - this allows all static members to be truly unique to the specific
    //   derived class
    // from http://stackoverflow.com/questions/424366/c-sharp-string-enums/424414#424414
    public class EnumBase<E, T> where E : EnumBase<E, T>
    {
        #region Instance code
        public T Value { get; private set; }
        public string Name { get; private set; }

        protected EnumBase(T EnumValue, string Name)
        {
            Value = EnumValue;
            this.Name = Name;
            mapping.Add(Name, this);
        }

        public override string ToString() { return Name; }
        #endregion

        #region Static tools
        static private readonly Dictionary<string, EnumBase<E, T>> mapping;
        static EnumBase()
        {
            mapping = new Dictionary<string, EnumBase<E, T>>();
        }
        protected static E Parse(string name)
        {
            EnumBase<E, T> result;
            if (mapping.TryGetValue(name, out result))
            {
                return (E)result;
            }

            throw new InvalidCastException();
        }
        // This is protected to force the child class to expose it's own static
        // method.
        // By recreating this static method at the derived class, static
        // initialization will be explicit, promising the mapping dictionary
        // will never be empty when this method is called.
        protected static IEnumerable<E> All
        {
            get
            {
                return mapping.Values.AsEnumerable().Cast<E>();
            }
        }
        #endregion
    }

    public sealed class CallRequestStatus : EnumBase<CallRequestStatus, int>
    {
        public static readonly CallRequestStatus ACTIVE = new CallRequestStatus(1, "ACTIVE");
        public static readonly CallRequestStatus FULFILLED = new CallRequestStatus(2, "FULFILLED");
        public static readonly CallRequestStatus DEACTIVATED = new CallRequestStatus(3, "DEACTIVATED");

        private CallRequestStatus(int Value, String Name) : base(Value, Name) { }
        public new static IEnumerable<CallRequestStatus> All
        {
            get
            { return EnumBase<CallRequestStatus, int>.All; }
        }

        public static explicit operator CallRequestStatus(string str)
        { return Parse(str); }
    }

    public sealed class ParticipantType : EnumBase<ParticipantType, int>
    {
        public static readonly ParticipantType PRODUCER = new ParticipantType(1, "PRODUCER");
        public static readonly ParticipantType CONSUMER = new ParticipantType(2, "CONSUMER");


        private ParticipantType(int Value, String Name) : base(Value, Name) { }
        public new static IEnumerable<ParticipantType> All
        { get { return EnumBase<ParticipantType, int>.All; } }

        public static explicit operator ParticipantType(string str)
        { return Parse(str); }



    }

    public sealed class MessageState
    {
        private readonly String name;
        private readonly int value;


        public static readonly MessageState UNSENT = new MessageState(1, "UNSENT");
        public static readonly MessageState SENT = new MessageState(2, "SENT");


        private MessageState(int value, String name)
        {
            this.name = name;
            this.value = value;

        }

        public override String ToString()
        {
            return name;
        }



    }

    public sealed class Method : EnumBase<Method, int>
    {
        public static readonly Method GET = new Method(1, "GET");
        public static readonly Method POST = new Method(2, "POST");
        public static readonly Method PUT = new Method(2, "PUT");

        private Method(int Value, String Name) : base(Value, Name) { }
        public new static IEnumerable<Method> All
        {
            get
            { return EnumBase<Method, int>.All; }
        }

        public static explicit operator Method(string str)
        { return Parse(str); }



    }
}