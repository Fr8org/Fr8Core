using System;
using System.Reflection;

namespace Daemons.EventExposers
{
    /// <summary>
    /// This class allows us to pseudo-statically type references to events.
    /// When you need to reference an event, create a new class in this folder, and implement the static accessors for the event.
    /// EventExposers should be named #NameOfClassWithDesiredEvent#EventExposer
    /// See the bottom of this file for an example.
    /// </summary>
    public abstract class ExposedEvent
    {
        private readonly String _name;
        private readonly Type _owner;

        protected ExposedEvent(string name, Type ownerType)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (ownerType == null) throw new ArgumentNullException("ownerType");

            _name = name;
            _owner = ownerType;
        }

        public static implicit operator EventInfo(ExposedEvent exposedEvent)
        {
            return exposedEvent._owner.GetEvent(exposedEvent._name);
        }
    }
    public abstract class ExposedEvent<T> : ExposedEvent
    {
        protected ExposedEvent(string name)
            : base(name, typeof(T))
        {
        }
    }

    /*  Example of ExposedEvent usage
     
        public class MySpecialClassWithEvents
        {
            public event EventHandler MyEvent;
        }

        public sealed class MySpecialClassWithEventsEventExposer : ExposedEvent<MySpecialClassWithEvents>
        {
            public static ExposedEvent MyEvent = new MySpecialClassWithEventsEventExposer("MyEvent");
            private MySpecialClassWithEventsEventExposer(string name)
                : base(name)
            {
            }
        }

        public class MyDaemon : Daemon
        {
            public MyDaemon()
            {
                RegisterEvent<EventArgs>(MySpecialClassWithEventsEventExposer.MyEvent, (a, b) => { });
            }
        }
    */
}
