using Data.Constants;
using Data.Interfaces.DataTransferObjects;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.Manifests
{
    public class CallStackConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var callStack = (OperationalStateCM.ActivityCallStack) value;

            writer.WriteStartArray();

            foreach (var item in callStack.Reverse())
            {
                serializer.Serialize(writer, item);
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = serializer.Deserialize<OperationalStateCM.StackFrame[]>(reader);
            var callStack = new OperationalStateCM.ActivityCallStack();

            foreach (var stackFrame in array)
            {
                callStack.Push(stackFrame);    
            }

            return callStack;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (OperationalStateCM.ActivityCallStack);
        }
    }

    

    public class OperationalStateCM : Manifest
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
            public StackLocalData LocalData { get; set; }
        }

        public class HistoryElement
        {
            public string Description { get; set; }
        }

        public class LoopStatus
        {
            public int Index { get; set; }
            public string CrateManifest { get; set; }
            public string Label { get; set; }
        }

        public class BranchStatus
        {
            public string Id { get; set; }
            public int Count { get; set; }
            public DateTime LastBranchTime { get; set; }
        }

        public ActivityCallStack CallStack { get; set; } = new ActivityCallStack();

        public List<HistoryElement> History { get; set; }
        public ActivityErrorCode? CurrentActivityErrorCode { get; set; }
        public string CurrentActivityErrorMessage { get; set; }
        public string CurrentClientActivityName { get; set; }
        public StackLocalData BypassData { get; set; }

        public ActivityResponseDTO CurrentActivityResponse { get; set; }

        public OperationalStateCM()
            : base(MT.OperationalStatus)
        {
            History = new List<HistoryElement>();
        }

        public T CreateLocalData<T>(string type)
            where T : new()
        {
            var data = new T();

            StoreLocalData(type, data);

            return data;
        }
        
        public T GetLocalData<T>(string type)
        {
            var top = CallStack.Peek();

            if (top.LocalData?.Type != type || top.LocalData?.Data == null)
            {
                return default(T);
            }

            return top.LocalData.ReadAs<T>();
        }

        public void StoreLocalData(string type, object data)
        {
            var top = CallStack.Peek();

            top.LocalData = new StackLocalData
            {
                Type = type,
                Data = JToken.FromObject(data)
            };
        }

        public T GetOrCreateLocalData<T>(string type)
            where T:new()
        {
            var top = CallStack.Peek();

            if (top.LocalData?.Type != type ||  top.LocalData?.Data == null)
            {
                return CreateLocalData<T>(type);
            }

            return top.LocalData.Data.ToObject<T>();
        }
    }
}
