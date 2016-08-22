namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemovePlanTemplateEntities : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActivityDescriptions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropForeignKey("dbo.NodeTransitions", "ActivityDescriptionId", "dbo.ActivityDescriptions");
            DropForeignKey("dbo.NodeTransitions", "PlanId", "dbo.Plans");
            DropForeignKey("dbo.PlanNodeDescriptions", "ActivityDescription_Id", "dbo.ActivityDescriptions");
            DropForeignKey("dbo.PlanNodeDescriptions", "ParentNodeId", "dbo.PlanNodeDescriptions");
            DropForeignKey("dbo.NodeTransitions", "PlanNodeDescriptionDO_Id", "dbo.PlanNodeDescriptions");
            DropForeignKey("dbo.PlanNodeDescriptions", "PlanTemplateDO_Id", "dbo.PlanTemplates");
            DropForeignKey("dbo.PlanTemplates", "Fr8AccountId", "dbo.Users");
            DropForeignKey("dbo.NodeTransitions", "PlanTemplateId", "dbo.PlanTemplates");
            DropIndex("dbo.ActivityDescriptions", new[] { "ActivityTemplateId" });
            DropIndex("dbo.NodeTransitions", new[] { "ActivityDescriptionId" });
            DropIndex("dbo.NodeTransitions", new[] { "PlanTemplateId" });
            DropIndex("dbo.NodeTransitions", new[] { "PlanId" });
            DropIndex("dbo.NodeTransitions", new[] { "PlanNodeDescriptionDO_Id" });
            DropIndex("dbo.PlanTemplates", new[] { "Fr8AccountId" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "ParentNodeId" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "ActivityDescription_Id" });
            DropIndex("dbo.PlanNodeDescriptions", new[] { "PlanTemplateDO_Id" });
            DropTable("dbo.ActivityDescriptions");
            DropTable("dbo.NodeTransitions");
            DropTable("dbo.PlanTemplates");
            DropTable("dbo.PlanNodeDescriptions");
            
            Sql("delete MtProperties from MtProperties inner join MtTypes on MtProperties.DeclaringType = MtTypes.Id where ManifestId = 38");
            Sql("delete MtData from MtData inner join MtTypes on MtData.Type = MtTypes.Id where ManifestId = 38");
            Sql("delete from MtTypes where ManifestId = 38");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.PlanNodeDescriptions", "PlanTemplateDO_Id");
            CreateIndex("dbo.PlanNodeDescriptions", "ActivityDescription_Id");
            CreateIndex("dbo.PlanNodeDescriptions", "ParentNodeId");
            CreateIndex("dbo.PlanTemplates", "Fr8AccountId");
            CreateIndex("dbo.NodeTransitions", "PlanNodeDescriptionDO_Id");
            CreateIndex("dbo.NodeTransitions", "PlanId");
            CreateIndex("dbo.NodeTransitions", "PlanTemplateId");
            CreateIndex("dbo.NodeTransitions", "ActivityDescriptionId");
            CreateIndex("dbo.ActivityDescriptions", "ActivityTemplateId");
            AddForeignKey("dbo.NodeTransitions", "PlanTemplateId", "dbo.PlanTemplates", "Id");
            AddForeignKey("dbo.PlanTemplates", "Fr8AccountId", "dbo.Users", "Id");
            AddForeignKey("dbo.PlanNodeDescriptions", "PlanTemplateDO_Id", "dbo.PlanTemplates", "Id");
            AddForeignKey("dbo.NodeTransitions", "PlanNodeDescriptionDO_Id", "dbo.PlanNodeDescriptions", "Id");
            AddForeignKey("dbo.PlanNodeDescriptions", "ParentNodeId", "dbo.PlanNodeDescriptions", "Id");
            AddForeignKey("dbo.PlanNodeDescriptions", "ActivityDescription_Id", "dbo.ActivityDescriptions", "Id");
            AddForeignKey("dbo.NodeTransitions", "PlanId", "dbo.Plans", "Id");
            AddForeignKey("dbo.NodeTransitions", "ActivityDescriptionId", "dbo.ActivityDescriptions", "Id");
            AddForeignKey("dbo.ActivityDescriptions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id", cascadeDelete: true);
        }
    }
}
