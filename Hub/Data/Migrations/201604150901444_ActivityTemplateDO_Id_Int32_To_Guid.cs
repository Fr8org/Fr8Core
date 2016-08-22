namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ActivityTemplateDO_Id_Int32_To_Guid : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });

            RenameTable("dbo.ActivityTemplate", "OldActivityTemplate");

            CreateTable(
                "dbo.ActivityTemplate",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    Label = c.String(),
                    Tags = c.String(),
                    Version = c.String(),
                    Description = c.String(),
                    NeedsAuthentication = c.Boolean(nullable: false),
                    ComponentActivities = c.String(),
                    ActivityTemplateState = c.Int(nullable: false),
                    TerminalId = c.Int(nullable: false),
                    Category = c.Int(nullable: false),
                    Type = c.Int(nullable: false),
                    MinPaneWidth = c.Int(nullable: false),
                    WebServiceId = c.Int(),
                    ClientVisibility = c.Boolean(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    OldId = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._ActivityTemplateStateTemplate", t => t.ActivityTemplateState, cascadeDelete: true)
                .ForeignKey("dbo.Terminals", t => t.TerminalId)
                .ForeignKey("dbo.WebServices", t => t.WebServiceId)
                .Index(t => t.ActivityTemplateState)
                .Index(t => t.TerminalId)
                .Index(t => t.WebServiceId);


            Sql(
                @"INSERT INTO [dbo].[ActivityTemplate] (
                    [Id],
                    [Name],
                    [Label],
                    [Tags],
                    [Version],
                    [Description],
                    [NeedsAuthentication],
                    [ComponentActivities],
                    [ActivityTemplateState],
                    [TerminalId],
                    [Category],
                    [Type],
                    [MinPaneWidth],
                    [WebServiceId],
                    [ClientVisibility],
                    [LastUpdated],
                    [CreateDate],
                    [OldId])
                SELECT
                    newid() as [Id],
                    [oc].[Name],
                    [oc].[Label],
                    [oc].[Tags],
                    [oc].[Version],
                    [oc].[Description],
                    [oc].[NeedsAuthentication],
                    [oc].[ComponentActivities],
                    [oc].[ActivityTemplateState],
                    [oc].[TerminalId],
                    [oc].[Category],
                    [oc].[Type],
                    [oc].[MinPaneWidth],
                    [oc].[WebServiceId],
                    [oc].[ClientVisibility],
                    [oc].[LastUpdated],
                    [oc].[CreateDate],
                    [oc].[Id] as [OldId]
                FROM [dbo].[OldActivityTemplate] AS [oc]"
            );

            RenameColumn("dbo.Actions", "ActivityTemplateId", "OldActivityTemplateId");
            AddColumn("dbo.Actions", "ActivityTemplateId", c => c.Guid(nullable: true));
            Sql("UPDATE [PN] SET [ActivityTemplateId] = [C].[Id] FROM [dbo].[Actions] AS [PN] INNER JOIN [dbo].[ActivityTemplate] [C] ON [C].[OldId] = [PN].[OldActivityTemplateId]");

            AlterColumn("dbo.Actions", "ActivityTemplateId", c => c.Guid(nullable: false));

            CreateIndex("dbo.Actions", "ActivityTemplateId");
            AddForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id");

            DropColumn("dbo.Actions", "OldActivityTemplateId");
            DropColumn("dbo.ActivityTemplate", "OldId");
            DropTable("dbo.OldActivityTemplate");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });

            RenameTable("dbo.ActivityTemplate", "OldActivityTemplate");

            CreateTable(
                "dbo.ActivityTemplate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Label = c.String(),
                    Tags = c.String(),
                    Version = c.String(),
                    Description = c.String(),
                    NeedsAuthentication = c.Boolean(nullable: false),
                    ComponentActivities = c.String(),
                    ActivityTemplateState = c.Int(nullable: false),
                    TerminalId = c.Int(nullable: false),
                    Category = c.Int(nullable: false),
                    Type = c.Int(nullable: false),
                    MinPaneWidth = c.Int(nullable: false),
                    WebServiceId = c.Int(),
                    ClientVisibility = c.Boolean(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    OldId = c.Guid(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._ActivityTemplateStateTemplate", t => t.ActivityTemplateState, cascadeDelete: true)
                .ForeignKey("dbo.Terminals", t => t.TerminalId)
                .ForeignKey("dbo.WebServices", t => t.WebServiceId)
                .Index(t => t.ActivityTemplateState)
                .Index(t => t.TerminalId)
                .Index(t => t.WebServiceId);


            Sql(
                @"INSERT INTO [dbo].[ActivityTemplate] (
                    [Name],
                    [Label],
                    [Tags],
                    [Version],
                    [Description],
                    [NeedsAuthentication],
                    [ComponentActivities],
                    [ActivityTemplateState],
                    [TerminalId],
                    [Category],
                    [Type],
                    [MinPaneWidth],
                    [WebServiceId],
                    [ClientVisibility],
                    [LastUpdated],
                    [CreateDate],
                    [OldId])
                SELECT
                    [oc].[Name],
                    [oc].[Label],
                    [oc].[Tags],
                    [oc].[Version],
                    [oc].[Description],
                    [oc].[NeedsAuthentication],
                    [oc].[ComponentActivities],
                    [oc].[ActivityTemplateState],
                    [oc].[TerminalId],
                    [oc].[Category],
                    [oc].[Type],
                    [oc].[MinPaneWidth],
                    [oc].[WebServiceId],
                    [oc].[ClientVisibility],
                    [oc].[LastUpdated],
                    [oc].[CreateDate],
                    [oc].[Id] as [OldId]
                FROM [dbo].[OldActivityTemplate] AS [oc]"
            );

            RenameColumn("dbo.Actions", "ActivityTemplateId", "OldActivityTemplateId");
            AddColumn("dbo.Actions", "ActivityTemplateId", c => c.Int(nullable: true));
            Sql("UPDATE [PN] SET [ActivityTemplateId] = [C].[Id] FROM [dbo].[Actions] AS [PN] INNER JOIN [dbo].[ActivityTemplate] [C] ON [C].[OldId] = [PN].[OldActivityTemplateId]");

            AlterColumn("dbo.Actions", "ActivityTemplateId", c => c.Int(nullable: false));

            CreateIndex("dbo.Actions", "ActivityTemplateId");
            AddForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id");

            DropColumn("dbo.Actions", "OldActivityTemplateId");
            DropColumn("dbo.ActivityTemplate", "OldId");
            DropTable("dbo.OldActivityTemplate");
        }
    }
}
