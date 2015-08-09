//using Newtonsoft.Json;
using Utilities.Serializers.Json;

namespace KwasantCore.Managers.APIManagers.Packagers.DataTable
{
    public class DataTablesPackager
    {
        private JsonSerializer jsonSerializer;

        public DataTablesPackager()
        {
            jsonSerializer = new JsonSerializer();
        }
        public string Pack(object dataObject)
        {
            return jsonSerializer.Serialize(dataObject);
        }
    }
}
