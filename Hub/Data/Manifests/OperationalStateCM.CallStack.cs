using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8Data.Manifests
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

            public StackFrame[] ToArray()
            {
                return _stack.ToArray();
            }

            public void CopyTo(Array array, int index)
            {
                ((ICollection) _stack).CopyTo(array, index);
            }

            public void CopyTo(StackFrame[] array, int arrayIndex)
            {
                _stack.CopyTo(array, arrayIndex);
            }

            // set of methods to work with activity's run-time state data.
            // It's better not to use these methods outside of very special internal activities
            public T CreateLocalData<T>(string type)
                where T : new()
            {
                var data = new T();

                StoreLocalData(type, data);

                return data;
            }

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

            public T GetOrCreateLocalData<T>(string type)
                where T : new()
            {
                var top = TopFrame;

                if (top.LocalData?.Type != type || top.LocalData?.Data == null)
                {
                    return CreateLocalData<T>(type);
                }

                return top.LocalData.Data.ToObject<T>();
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
            // Here activity can store some custom state related to it's execution. It is better to avoid persisting such states if possible. 
            // Legacy notes: This is a replacement for LoopStatus and Branches arrays we previously had
            public StackLocalData LocalData { get; set; } 
        }
    }
}
