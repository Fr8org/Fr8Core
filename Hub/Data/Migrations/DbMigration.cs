using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Migrations
{
    public abstract class DbMigration : System.Data.Entity.Migrations.DbMigration
    {
        protected void SeedConstants<T>(string tableName, bool setIdentityInsertOn = false)
        {
            if (setIdentityInsertOn)
            {
                Sql(string.Format("SET IDENTITY_INSERT {0} ON;", tableName));
            }
            foreach (var fieldInfo in typeof(T).GetFields())
            {
                Sql(string.Format("INSERT INTO {0} (Id, Name) VALUES ({1}, '{2}')", tableName, fieldInfo.GetValue(null), fieldInfo.Name));
            }
            if (setIdentityInsertOn)
            {
                Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", tableName));
            }
        }
    }
}
