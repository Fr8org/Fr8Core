using System;
using NUnit.Framework;

namespace pluginAzureSqlServerTests
{
    [TestFixture]
    public class CanSaveDataToSql
    {
        [SetUp]
        public void Init()
        {
            var helper = new TestDbHelper();
            using (var dbconn = helper.CreateConnection())
            using (var tx = dbconn.BeginTransaction())
            {
                if (helper.CustomersTableExists(tx))
                {
                    helper.DropCustomersTable(tx);
                }

                helper.CreateCustomersTable(tx);

                tx.Commit();
            }
        }

        [TearDown]
        public void Cleanup()
        {
            var helper = new TestDbHelper();
            using (var dbconn = helper.CreateConnection())
            using (var tx = dbconn.BeginTransaction())
            {
                if (helper.CustomersTableExists(tx))
                {
                    helper.DropCustomersTable(tx);
                }

                tx.Commit();
            }
        }

        [Test]
        public void CallCommandWrite()
        {
        }
    }
}
