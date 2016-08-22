namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Container_Id_Int32_To_Guid : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers");
            DropForeignKey("dbo.ProcessNodes", "ParentProcessId", "dbo.Processes");
            DropIndex("dbo.ProcessNodes", new[] { "ParentContainerId" });

            RenameTable("dbo.Containers", "OldContainers");

            CreateTable(
                "dbo.Containers",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    ContainerState = c.Int(nullable: false),
                    CrateStorage = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    RouteId = c.Int(nullable: false),
                    CurrentRouteNodeId = c.Int(),
                    NextRouteNodeId = c.Int(),
                    OldId = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Routes", t => t.RouteId)
                .ForeignKey("dbo.RouteNodes", t => t.CurrentRouteNodeId)
                .ForeignKey("dbo.RouteNodes", t => t.NextRouteNodeId)
                .ForeignKey("dbo._ContainerStateTemplate", t => t.ContainerState, cascadeDelete: true)
                .Index(t => t.RouteId)
                .Index(t => t.ContainerState)
                .Index(t => t.CurrentRouteNodeId)
                .Index(t => t.NextRouteNodeId);

            Sql(
                @"INSERT INTO [dbo].[Containers] (
                    [Id],
                    [Name],
                    [ContainerState],
                    [CrateStorage],
                    [LastUpdated],
                    [CreateDate],
                    [RouteId],
                    [CurrentRouteNodeId],
                    [NextRouteNodeId],
                    [OldId])
                SELECT
                    newid() as [Id],
                    [oc].[Name],
                    [oc].[ContainerState],
                    [oc].[CrateStorage],
                    [oc].[LastUpdated],
                    [oc].[CreateDate],
                    [oc].[RouteId],
                    [oc].[CurrentRouteNodeId],
                    [oc].[NextRouteNodeId],
                    [oc].[Id] as [OldId]
                FROM [dbo].[OldContainers] AS [oc]"
            );

            RenameColumn("dbo.ProcessNodes", "ParentContainerId", "OldParentContainerId");
            AddColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Guid(nullable: true));
            Sql("UPDATE [PN] SET [ParentContainerId] = [C].[Id] FROM [dbo].[ProcessNodes] AS [PN] INNER JOIN [dbo].[Containers] [C] ON [C].[OldId] = [PN].[OldParentContainerId]");
            AlterColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Guid(nullable: false));

            CreateIndex("dbo.ProcessNodes", "ParentContainerId");
            AddForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers", "Id");

            DropColumn("dbo.ProcessNodes", "OldParentContainerId");
            DropColumn("dbo.Containers", "OldId");
            DropTable("dbo.OldContainers");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers");
            DropIndex("dbo.ProcessNodes", new[] { "ParentContainerId" });

            RenameTable("dbo.Containers", "OldContainers");

            CreateTable(
                "dbo.Containers",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    ContainerState = c.Int(nullable: false),
                    CrateStorage = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    RouteId = c.Int(nullable: false),
                    CurrentRouteNodeId = c.Int(),
                    NextRouteNodeId = c.Int(),
                    OldId = c.Guid(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Routes", t => t.RouteId)
                .ForeignKey("dbo.RouteNodes", t => t.CurrentRouteNodeId)
                .ForeignKey("dbo.RouteNodes", t => t.NextRouteNodeId)
                .ForeignKey("dbo._ContainerStateTemplate", t => t.ContainerState, cascadeDelete: true)
                .Index(t => t.RouteId)
                .Index(t => t.ContainerState)
                .Index(t => t.CurrentRouteNodeId)
                .Index(t => t.NextRouteNodeId);

            Sql(
                @"INSERT INTO [dbo].[Containers] (
                    [Name],
                    [ContainerState],
                    [CrateStorage],
                    [LastUpdated],
                    [CreateDate],
                    [RouteId],
                    [CurrentRouteNodeId],
                    [NextRouteNodeId],
                    [OldId])
                SELECT
                    [oc].[Name],
                    [oc].[ContainerState],
                    [oc].[CrateStorage],
                    [oc].[LastUpdated],
                    [oc].[CreateDate],
                    [oc].[RouteId],
                    [oc].[CurrentRouteNodeId],
                    [oc].[NextRouteNodeId],
                    [oc].[Id] as [OldId]
                FROM [dbo].[OldContainers] AS [oc]"
            );

            RenameColumn("dbo.ProcessNodes", "ParentContainerId", "OldParentContainerId");
            AddColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Int(nullable: true));
            Sql("UPDATE [PN] SET [ParentContainerId] = [C].[Id] FROM [dbo].[ProcessNodes] AS [PN] INNER JOIN [dbo].[Containers] [C] ON [C].[OldId] = [PN].[OldParentContainerId]");
            AlterColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Int(nullable: false));

            CreateIndex("dbo.ProcessNodes", "ParentContainerId");
            AddForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers", "Id");

            DropColumn("dbo.ProcessNodes", "OldParentContainerId");
            DropColumn("dbo.Containers", "OldId");
            DropTable("dbo.OldContainers");
        }
    }
}
