using System;
using System.Linq;
using Newtonsoft.Json;

namespace Fr8Data.Manifests
{
    partial class OperationalStateCM
    {
        // we need this to correctly serialize stack. Otherwise all elements get reversed after deserialization
        public class CallStackConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var callStack = (OperationalStateCM.ActivityCallStack)value;

                if (callStack == null)
                {
                    writer.WriteNull();
                    return;
                }

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

                if (array == null)
                {
                    return null;
                }

                var callStack = new OperationalStateCM.ActivityCallStack();

                foreach (var stackFrame in array)
                {
                    callStack.PushFrame(stackFrame);
                }

                return callStack;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(OperationalStateCM.ActivityCallStack);
            }
        }
    }
}
