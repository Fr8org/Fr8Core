using Fr8.Infrastructure.Utilities.Serializers.Json;

namespace Hub.Managers.APIManagers.Packagers.DataTable
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
