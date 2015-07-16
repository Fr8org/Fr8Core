using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Plugins.AzureSql
{
    public class AzureSqlPlugin : Plugin
    {
        public void WriteCommand(WriteCommandArgs args)
        {
        }

        protected IDbConnection CreateConnection(string provider, string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
