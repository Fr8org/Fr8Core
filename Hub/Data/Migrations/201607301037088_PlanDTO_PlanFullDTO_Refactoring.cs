namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanDTO_PlanFullDTO_Refactoring : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("delete MtProperties from MtProperties inner join MtTypes on MtProperties.DeclaringType = MtTypes.Id where ManifestId = 38");
            Sql("delete MtData from MtData inner join MtTypes on MtData.Type = MtTypes.Id where ManifestId = 38");
            Sql("delete from MtTypes where ManifestId = 38");
        }
        
        public override void Down()
        {
        }
    }
}
