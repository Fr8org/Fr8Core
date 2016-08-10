using Fr8.Infrastructure.Utilities;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Standard_Configuration_Controls_To_Standard_UI_Controls : System.Data.Entity.Migrations.DbMigration
    {
        private string oldValue = "Standard Configuration Controls";
        private string newValue = "Standard UI Controls";
        public override void Up()
        {
            UpdateTable_Contrainer_Attribute_CrateStorage(oldValue, newValue);
        }
        
        public override void Down()
        {
            UpdateTable_Contrainer_Attribute_CrateStorage(newValue, oldValue);
        }

        private void UpdateTable_Contrainer_Attribute_CrateStorage(string oldValue, string newValue)
        {
            var sqlCommand = @"UPDATE [act]
                SET [act].[CrateStorage] = REPLACE([act].[CrateStorage], '{0}', '{1}')
                FROM [dbo].[Actions] AS [act]
                WHERE [act].[CrateStorage] LIKE '%{0}%'".format(oldValue, newValue);
            Sql(sqlCommand);
        }
    }
}
