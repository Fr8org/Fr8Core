using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.Manifests
{
    partial class OperationalStateCM
    {
        [JsonConverter(typeof(CallStackConverter))]
        public class ActivityCallStack : IEnumerable<StackFrame>
        {
            private readonly Stack<StackFrame> _stack = new Stack<StackFrame>();
            
            public int Count => _stack.Count;

            public StackFrame TopFrame
            {
                get
                {
                    if (_stack.Count == 0)
                    {
                        return null;
                    }

                    return _stack.Peek();
                }
            }

            public void RemoveTopFrame()
            {
                _stack.Pop();
            }
            
            public void PushFrame(StackFrame frame)
            {
                _stack.Push(frame);
            }

            public void Clear()
            {
                _stack.Clear();
            }
            
            // set of methods to work with activity's run-time state data.
            
            public T GetLocalData<T>(string type)
            {
                var top = TopFrame;

                if (top.LocalData?.Type != type || top.LocalData?.Data == null)
                {
                    return default(T);
                }

                return top.LocalData.ReadAs<T>();
            }

            public void StoreLocalData(string type, object data)
            {
                var top = TopFrame;

                top.LocalData = new StackLocalData
                {
                    Type = type,
                    Data = JToken.FromObject(data)
                };
            }
            
            public IEnumerator<StackFrame> GetEnumerator()
            {
                return _stack.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_stack).GetEnumerator();
            }
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
            // Here activity can store some custom state related to it's execution.
            // Legacy notes: This is a replacement for LoopStatus and Branches arrays we previously had
            public StackLocalData LocalData { get; set; } 
        }
    }
}
