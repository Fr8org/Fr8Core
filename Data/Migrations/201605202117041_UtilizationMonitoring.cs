namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UtilizationMonitoring : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
               "dbo.UtilizationMetrics",
               c => new
               {
                   UserId = c.String(nullable: false, maxLength:128),
                   Epu = c.Int(nullable: false),
                   LastUpdated = c.DateTimeOffset(nullable:false, precision:7),
                
               })
               .PrimaryKey(t => t.UserId);

            CreateTable(
               "dbo.RateLimiterState",
               c => new
               {
                   UserId = c.String(nullable: false, maxLength: 128),
                   IsOverheating = c.Int(nullable: false),
                   BlockTill = c.DateTimeOffset(nullable: true, precision: 7),

               })
               .PrimaryKey(t => t.UserId);
        }
        
        public override void Down()
        {
            DropTable("dbo.UtilizationMetrics");
            DropTable("dbo.RateLimiterState");
        }
    }
}
