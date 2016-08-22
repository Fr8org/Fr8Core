namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Plan_Templates : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActivityDescriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginalId = c.String(),
                        Name = c.String(),
                        Version = c.String(),
                        ActivityTemplateId = c.Guid(nullable: false),
                        CrateStorage = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityTemplate", t => t.ActivityTemplateId, cascadeDelete: true)
                .Index(t => t.ActivityTemplateId);
            
            CreateTable(
                "dbo.NodeTransitions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Transition = c.Int(nullable: false),
                        ActivityDescriptionId = c.Int(),
                        PlanTemplateId = c.Int(),
                        PlanId = c.Guid(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        PlanNodeDescriptionDO_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDescriptions", t => t.ActivityDescriptionId)
                .ForeignKey("dbo.Plans", t => t.PlanId)
                .ForeignKey("dbo.PlanNodeDescriptions", t => t.PlanNodeDescriptionDO_Id)
                .ForeignKey("dbo.PlanTemplates", t => t.PlanTemplateId)
                .Index(t => t.ActivityDescriptionId)
                .Index(t => t.PlanTemplateId)
                .Index(t => t.PlanId)
                .Index(t => t.PlanNodeDescriptionDO_Id);
            
            CreateTable(
                "dbo.PlanTemplates",
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
                        SubPlanOriginalId = c.String(),
                        IsStartingSubplan = c.Boolean(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        ActivityDescription_Id = c.Int(),
                        PlanTemplateDO_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDescriptions", t => t.ActivityDescription_Id)
                .ForeignKey("dbo.PlanNodeDescriptions", t => t.ParentNodeId)
                .ForeignKey("dbo.PlanTemplates", t => t.PlanTemplateDO_Id)
                .Index(t => t.ParentNodeId)
                .Index(t => t.ActivityDescription_Id)
                .Index(t => t.PlanTemplateDO_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NodeTransitions", "PlanTemplateId", "dbo.PlanTemplates");
            DropForeignKey("dbo.PlanTemplates", "Fr8AccountId", "dbo.Users");
            DropForeignKey("dbo.PlanNodeDescriptions", "PlanTemplateDO_Id", "dbo.PlanTemplates");
            DropForeignKey("dbo.NodeTransitions", "PlanNodeDescriptionDO_Id", "dbo.PlanNodeDescriptions");
            DropForeignKey("dbo.PlanNodeDescriptions", "ParentNodeId", "dbo.PlanNodeDescriptions");
            DropForeignKey("dbo.PlanNodeDescriptions", "ActivityDescription_Id", "dbo.ActivityDescriptions");
            DropForeignKey("dbo.NodeTransitions", "PlanId", "dbo.Plans");
            DropForeignKey("dbo.NodeTransitions", "ActivityDescriptionId", "dbo.ActivityDescriptions");
            DropForeignKey("dbo.ActivityDescriptions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.PlanNodeDescriptions", new[] { "PlanTemplateDO_Id" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "ActivityDescription_Id" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "ParentNodeId" });
            DropIndex("dbo.PlanTemplates", new[] { "Fr8AccountId" });
            DropIndex("dbo.NodeTransitions", new[] { "PlanNodeDescriptionDO_Id" });
            DropIndex("dbo.NodeTransitions", new[] { "PlanId" });
            DropIndex("dbo.NodeTransitions", new[] { "PlanTemplateId" });
            DropIndex("dbo.NodeTransitions", new[] { "ActivityDescriptionId" });
            DropIndex("dbo.ActivityDescriptions", new[] { "ActivityTemplateId" });
            DropTable("dbo.PlanNodeDescriptions");
            DropTable("dbo.PlanTemplates");
            DropTable("dbo.NodeTransitions");
            DropTable("dbo.ActivityDescriptions");
        }
    }
}
