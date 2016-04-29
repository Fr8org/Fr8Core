namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanDescription : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActivityDescriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Version = c.String(),
                        ActivityTemplateId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        CrateStorage = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityTemplate", t => t.ActivityTemplateId, cascadeDelete: true)
                .Index(t => t.ActivityTemplateId);
            
            CreateTable(
                "dbo.ActivityTransitions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Transition = c.Int(nullable: false),
                        ActivityDescriptionId = c.Int(),
                        PlanDescriptionId = c.Int(),
                        PlanId = c.Guid(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        PlanNodeDescriptionDO_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDescriptions", t => t.ActivityDescriptionId)
                .ForeignKey("dbo.Plans", t => t.PlanId)
                .ForeignKey("dbo.PlanNodeDescriptions", t => t.PlanNodeDescriptionDO_Id)
                .ForeignKey("dbo.PlanDescription", t => t.PlanDescriptionId)
                .Index(t => t.ActivityDescriptionId)
                .Index(t => t.PlanDescriptionId)
                .Index(t => t.PlanId)
                .Index(t => t.PlanNodeDescriptionDO_Id);
            
            CreateTable(
                "dbo.PlanDescription",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Fr8AccountId = c.String(maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Fr8AccountId)
                .Index(t => t.Fr8AccountId);
            
            CreateTable(
                "dbo.PlanNodeDescriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ParentNodeId = c.Int(),
                        SubPlanName = c.String(),
                        IsStartingSubplan = c.Boolean(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        ActivityDescription_Id = c.Int(),
                        PlanDescriptionDO_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDescriptions", t => t.ActivityDescription_Id)
                .ForeignKey("dbo.PlanNodeDescriptions", t => t.ParentNodeId)
                .ForeignKey("dbo.PlanDescription", t => t.PlanDescriptionDO_Id)
                .Index(t => t.ParentNodeId)
                .Index(t => t.ActivityDescription_Id)
                .Index(t => t.PlanDescriptionDO_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActivityTransitions", "PlanDescriptionId", "dbo.PlanDescription");
            DropForeignKey("dbo.PlanDescription", "Fr8AccountId", "dbo.Users");
            DropForeignKey("dbo.PlanNodeDescriptions", "PlanDescriptionDO_Id", "dbo.PlanDescription");
            DropForeignKey("dbo.ActivityTransitions", "PlanNodeDescriptionDO_Id", "dbo.PlanNodeDescriptions");
            DropForeignKey("dbo.PlanNodeDescriptions", "ParentNodeId", "dbo.PlanNodeDescriptions");
            DropForeignKey("dbo.PlanNodeDescriptions", "ActivityDescription_Id", "dbo.ActivityDescriptions");
            DropForeignKey("dbo.ActivityTransitions", "PlanId", "dbo.Plans");
            DropForeignKey("dbo.ActivityTransitions", "ActivityDescriptionId", "dbo.ActivityDescriptions");
            DropForeignKey("dbo.ActivityDescriptions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.PlanNodeDescriptions", new[] { "PlanDescriptionDO_Id" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "ActivityDescription_Id" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "ParentNodeId" });
            DropIndex("dbo.PlanDescription", new[] { "Fr8AccountId" });
            DropIndex("dbo.ActivityTransitions", new[] { "PlanNodeDescriptionDO_Id" });
            DropIndex("dbo.ActivityTransitions", new[] { "PlanId" });
            DropIndex("dbo.ActivityTransitions", new[] { "PlanDescriptionId" });
            DropIndex("dbo.ActivityTransitions", new[] { "ActivityDescriptionId" });
            DropIndex("dbo.ActivityDescriptions", new[] { "ActivityTemplateId" });
            DropTable("dbo.PlanNodeDescriptions");
            DropTable("dbo.PlanDescription");
            DropTable("dbo.ActivityTransitions");
            DropTable("dbo.ActivityDescriptions");
        }
    }
}
