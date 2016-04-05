using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.Manifests
{
    partial class OperationalStateCM
    {
        [JsonConverter(typeof(CallStackConverter))]
        public class ActivityCallStack : Stack<StackFrame>
        {
        }

        public enum ActivityExecutionPhase
        {
            WasNotExecuted = 0,
            ProcessingChildren = 1
        }

        public class StackLocalData
        {
            public string Type;
            public JToken Data;


            public T ReadAs<T>()
            {
                return Data == null ? default(T) : Data.ToObject<T>();
            }
        }

        public class StackFrame
        {
            public Guid NodeId { get; set; }
            public string NodeName { get; set; }
            public ActivityExecutionPhase CurrentActivityExecutionPhase { get; set; }
            public Guid? CurrentChildId { get; set; }
            // Here activity can store some custom state related to it's execution. It is better to avoid persisting such states if possible. 
            // Legacy notes: This is a replacement for LoopStatus and Branches arrays we previously had
            public StackLocalData LocalData { get; set; } 
        }
    }
}
