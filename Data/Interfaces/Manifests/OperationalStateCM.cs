using Data.Constants;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.Manifests
{
    public partial class OperationalStateCM : Manifest
    {
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
