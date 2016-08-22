namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RouteNode_Id_Int32_To_Guid : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKeys();
            RouteNodeTable_Int32ToGuid();
            ContainerForeignKeys_Int32ToGuid();
            CriteriaForeignKeys_Int32ToGuid();
            ProcessNodesForeignKeys_Int32ToGuid();
            ActionsTable_Int32ToGuid();
            RoutesTable_Int32ToGuid();
            SubroutesTable_Int32ToGuid();
            AddForeignKeys();
            DropTemporaryTablesAndColumns();
        }

        public override void Down()
        {
            DropForeignKeys();
            RouteNodeTable_GuidToInt32();
            ContainerForeignKeys_GuidToInt32();
            CriteriaForeignKeys_GuidToInt32();
            ProcessNodesForeignKeys_GuidToInt32();
            ActionsTable_GuidToInt32();
            RoutesTable_GuidToInt32();
            SubroutesTable_GuidToInt32();
            AddForeignKeys();
            DropTemporaryTablesAndColumns();
        }

        private void DropForeignKeys()
        {
            DropForeignKey("dbo.RouteNodes", "ParentRouteNodeId", "dbo.RouteNodes");
            DropForeignKey("dbo.Containers", "CurrentRouteNodeId", "dbo.RouteNodes");
            DropForeignKey("dbo.Containers", "NextRouteNodeId", "dbo.RouteNodes");
            DropForeignKey("dbo.Routes", "Id", "dbo.RouteNodes");
            DropForeignKey("dbo.Actions", "Id", "dbo.RouteNodes");
            DropForeignKey("dbo.Subroutes", "Id", "dbo.RouteNodes");
            DropForeignKey("dbo.Containers", "RouteId", "dbo.Routes");
            DropForeignKey("dbo.Criteria", "SubrouteId", "dbo.Subroutes");
            DropForeignKey("dbo.ProcessNodes", "SubrouteId", "dbo.Subroutes");
            DropIndex("dbo.Containers", new[] { "RouteId" });
            DropIndex("dbo.Containers", new[] { "CurrentRouteNodeId" });
            DropIndex("dbo.Containers", new[] { "NextRouteNodeId" });
            DropIndex("dbo.RouteNodes", new[] { "ParentRouteNodeId" });
            DropIndex("dbo.Criteria", new[] { "SubrouteId" });
            DropIndex("dbo.ProcessNodes", new[] { "SubrouteId" });
            DropIndex("dbo.Routes", new[] { "Id" });
            DropIndex("dbo.Actions", new[] { "Id" });
            DropIndex("dbo.Subroutes", new[] { "Id" });
        }

        private void AddForeignKeys()
        {
            CreateIndex("dbo.Containers", "RouteId");
            CreateIndex("dbo.Containers", "CurrentRouteNodeId");
            CreateIndex("dbo.Containers", "NextRouteNodeId");
            CreateIndex("dbo.Criteria", "SubrouteId");
            CreateIndex("dbo.ProcessNodes", "SubrouteId");

            AddForeignKey("dbo.Containers", "RouteId", "dbo.Routes", "Id");
            AddForeignKey("dbo.Containers", "CurrentRouteNodeId", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Containers", "NextRouteNodeId", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Criteria", "SubrouteId", "dbo.Subroutes", "Id");
            AddForeignKey("dbo.ProcessNodes", "SubrouteId", "dbo.Subroutes", "Id");
        }

        private void DropTemporaryTablesAndColumns()
        {
            DropTable("dbo.OldSubroutes");
            DropTable("dbo.OldRoutes");
            DropTable("dbo.OldActions");
            DropTable("dbo.OldRouteNodes");
            DropColumn("dbo.RouteNodes", "OldId");
            DropColumn("dbo.Containers", "OldRouteId");
            DropColumn("dbo.Containers", "OldCurrentRouteNodeId");
            DropColumn("dbo.Containers", "OldNextRouteNodeId");
            DropColumn("dbo.Criteria", "OldSubrouteId");
            DropColumn("dbo.ProcessNodes", "OldSubrouteId");
        }

        private void RouteNodeTable_Int32ToGuid()
        {
            RenameTable("dbo.RouteNodes", "OldRouteNodes");

            CreateTable(
                "dbo.RouteNodes",
                c => new
                {
                    Id = c.Guid(nullable: false),  // New Guid column.
                    ParentRouteNodeId = c.Guid(),
                    Ordering = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    OldId = c.Int(nullable: false)  // We want to keep old id value too, in order to replaced referenced values.
                })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.RouteNodes", t => t.ParentRouteNodeId)
            .Index(t => t.ParentRouteNodeId);

            Sql(
                @"INSERT INTO [dbo].[RouteNodes] (
                    [Id],
                    [ParentRouteNodeId],
                    [Ordering],
                    [LastUpdated],
                    [CreateDate],
                    [OldId])
                SELECT
                    newid() as [Id],
                    null as [ParentRouteNodeId],
                    [orn].[Ordering],
                    [orn].[LastUpdated],
                    [orn].[CreateDate],
                    [orn].[Id] as [OldId]
                FROM [dbo].[OldRouteNodes] AS [orn]"
            );

            Sql(
                @"UPDATE [rn]
                SET [rn].[ParentRouteNodeId] = [rnp].[Id]
                FROM [dbo].[RouteNodes] AS [rn]
                INNER JOIN [dbo].[OldRouteNodes] AS [orn]
                    ON [orn].[Id] = [rn].[OldId]
                LEFT JOIN [dbo].[OldRouteNodes] AS [ornp]
                    ON [ornp].[Id] = [orn].[ParentRouteNodeId]
                LEFT JOIN [dbo].[RouteNodes] AS [rnp]
                    ON [rnp].[OldId] = [ornp].[Id]"
            );
        }

        private void RouteNodeTable_GuidToInt32()
        {
            RenameTable("dbo.RouteNodes", "OldRouteNodes");

            CreateTable(
                "dbo.RouteNodes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ParentRouteNodeId = c.Int(),
                    Ordering = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    OldId = c.Guid(nullable: false)
                })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.RouteNodes", t => t.ParentRouteNodeId)
            .Index(t => t.ParentRouteNodeId);

            Sql(
                @"INSERT INTO [dbo].[RouteNodes] (
                    [ParentRouteNodeId],
                    [Ordering],
                    [LastUpdated],
                    [CreateDate],
                    [OldId])
                SELECT
                    null as [ParentRouteNodeId],
                    [orn].[Ordering],
                    [orn].[LastUpdated],
                    [orn].[CreateDate],
                    [orn].[Id] as [OldId]
                FROM [dbo].[OldRouteNodes] AS [orn]"
            );

            Sql(
                @"UPDATE [rn]
                SET [rn].[ParentRouteNodeId] = [rnp].[Id]
                FROM [dbo].[RouteNodes] AS [rn]
                INNER JOIN [dbo].[OldRouteNodes] AS [orn]
                    ON [orn].[Id] = [rn].[OldId]
                LEFT JOIN [dbo].[OldRouteNodes] AS [ornp]
                    ON [ornp].[Id] = [orn].[ParentRouteNodeId]
                LEFT JOIN [dbo].[RouteNodes] AS [rnp]
                    ON [rnp].[OldId] = [ornp].[Id]"
            );
        }

        private void ContainerForeignKeys_Int32ToGuid()
        {
            // dbo.Containers#RouteId
            RenameColumn("dbo.Containers", "RouteId", "OldRouteId");
            AddColumn("dbo.Containers", "RouteId", c => c.Guid(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[RouteId] = [rn].[Id]
                FROM [dbo].[Containers] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldRouteId] = [rn].[OldId]"
            );

            AlterColumn("dbo.Containers", "RouteId", c => c.Guid(nullable: false));

            // dbo.Containers#CurrentRouteNodeId
            RenameColumn("dbo.Containers", "CurrentRouteNodeId", "OldCurrentRouteNodeId");
            AddColumn("dbo.Containers", "CurrentRouteNodeId", c => c.Guid(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[CurrentRouteNodeId] = [rn].[Id]
                FROM [dbo].[Containers] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldCurrentRouteNodeId] = [rn].[OldId]"
            );

            // dbo.Containers#NextRouteNodeId
            RenameColumn("dbo.Containers", "NextRouteNodeId", "OldNextRouteNodeId");
            AddColumn("dbo.Containers", "NextRouteNodeId", c => c.Guid(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[NextRouteNodeId] = [rn].[Id]
                FROM [dbo].[Containers] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldNextRouteNodeId] = [rn].[OldId]"
            );
        }

        private void ContainerForeignKeys_GuidToInt32()
        {
            // dbo.Containers#RouteId
            RenameColumn("dbo.Containers", "RouteId", "OldRouteId");
            AddColumn("dbo.Containers", "RouteId", c => c.Int(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[RouteId] = [rn].[Id]
                FROM [dbo].[Containers] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldRouteId] = [rn].[OldId]"
            );

            AlterColumn("dbo.Containers", "RouteId", c => c.Int(nullable: false));

            // dbo.Containers#CurrentRouteNodeId
            RenameColumn("dbo.Containers", "CurrentRouteNodeId", "OldCurrentRouteNodeId");
            AddColumn("dbo.Containers", "CurrentRouteNodeId", c => c.Int(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[CurrentRouteNodeId] = [rn].[Id]
                FROM [dbo].[Containers] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldCurrentRouteNodeId] = [rn].[OldId]"
            );

            // dbo.Containers#NextRouteNodeId
            RenameColumn("dbo.Containers", "NextRouteNodeId", "OldNextRouteNodeId");
            AddColumn("dbo.Containers", "NextRouteNodeId", c => c.Int(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[NextRouteNodeId] = [rn].[Id]
                FROM [dbo].[Containers] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldNextRouteNodeId] = [rn].[OldId]"
            );
        }

        private void CriteriaForeignKeys_Int32ToGuid()
        {
            // dbo.Criteria#SubrouteId
            RenameColumn("dbo.Criteria", "SubrouteId", "OldSubrouteId");
            AddColumn("dbo.Criteria", "SubrouteId", c => c.Guid(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[SubrouteId] = [rn].[Id]
                FROM [dbo].[Criteria] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldSubrouteId] = [rn].[OldId]"
            );
        }

        private void CriteriaForeignKeys_GuidToInt32()
        {
            // dbo.Criteria#SubrouteId
            RenameColumn("dbo.Criteria", "SubrouteId", "OldSubrouteId");
            AddColumn("dbo.Criteria", "SubrouteId", c => c.Int(nullable: true));

            Sql(
                @"UPDATE [c]
                SET [c].[SubrouteId] = [rn].[Id]
                FROM [dbo].[Criteria] AS [c]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [c].[OldSubrouteId] = [rn].[OldId]"
            );
        }

        private void ProcessNodesForeignKeys_Int32ToGuid()
        {
            // AlterColumn("dbo.ProcessNodes", "SubrouteId", c => c.Guid(nullable: false));
            // dbo.ProcessNodes#SubrouteId
            RenameColumn("dbo.ProcessNodes", "SubrouteId", "OldSubrouteId");
            AddColumn("dbo.ProcessNodes", "SubrouteId", c => c.Guid(nullable: true));

            Sql(
                @"UPDATE [pn]
                SET [pn].[SubrouteId] = [rn].[Id]
                FROM [dbo].[ProcessNodes] AS [pn]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [pn].[OldSubrouteId] = [rn].[OldId]"
            );

            AlterColumn("dbo.ProcessNodes", "SubrouteId", c => c.Guid(nullable: false));
        }

        private void ProcessNodesForeignKeys_GuidToInt32()
        {
            // AlterColumn("dbo.ProcessNodes", "SubrouteId", c => c.Guid(nullable: false));
            // dbo.ProcessNodes#SubrouteId
            RenameColumn("dbo.ProcessNodes", "SubrouteId", "OldSubrouteId");
            AddColumn("dbo.ProcessNodes", "SubrouteId", c => c.Int(nullable: true));

            Sql(
                @"UPDATE [pn]
                SET [pn].[SubrouteId] = [rn].[Id]
                FROM [dbo].[ProcessNodes] AS [pn]
                LEFT JOIN [dbo].[RouteNodes] AS [rn]
                    ON [pn].[OldSubrouteId] = [rn].[OldId]"
            );

            AlterColumn("dbo.ProcessNodes", "SubrouteId", c => c.Int(nullable: false));
        }

        private void ActionsTable_Int32ToGuid()
        {
            RenameTable("dbo.Actions", "OldActions");

            CreateTable(
                "dbo.Actions",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    CrateStorage = c.String(),
                    ActivityTemplateId = c.Int(),
                    currentView = c.String(),
                    Label = c.String()
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.Id)
                .ForeignKey("dbo.ActivityTemplate", t => t.ActivityTemplateId)
                .Index(t => t.Id)
                .Index(t => t.ActivityTemplateId);

            Sql(
                @"INSERT INTO [dbo].[Actions] (
                    [Id],
                    [Name],
                    [CrateStorage],
                    [ActivityTemplateId],
                    [currentView],
                    [Label])
                SELECT
                    [rn].[Id] as [Id],
                    [oa].[Name],
                    [oa].[CrateStorage],
                    [oa].[ActivityTemplateId],
                    [oa].[currentView],
                    [oa].[Label]
                FROM [dbo].[OldActions] AS [oa]
                INNER JOIN [dbo].[RouteNodes] [rn]
                    ON [rn].[OldId] = [oa].[Id]"
            );
        }

        private void ActionsTable_GuidToInt32()
        {
            RenameTable("dbo.Actions", "OldActions");

            CreateTable(
                "dbo.Actions",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                    CrateStorage = c.String(),
                    ActivityTemplateId = c.Int(),
                    currentView = c.String(),
                    Label = c.String()
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.Id)
                .ForeignKey("dbo.ActivityTemplate", t => t.ActivityTemplateId)
                .Index(t => t.Id)
                .Index(t => t.ActivityTemplateId);

            Sql(
                @"INSERT INTO [dbo].[Actions] (
                    [Id],
                    [Name],
                    [CrateStorage],
                    [ActivityTemplateId],
                    [currentView],
                    [Label])
                SELECT
                    [rn].[Id] as [Id],
                    [oa].[Name],
                    [oa].[CrateStorage],
                    [oa].[ActivityTemplateId],
                    [oa].[currentView],
                    [oa].[Label]
                FROM [dbo].[OldActions] AS [oa]
                INNER JOIN [dbo].[RouteNodes] [rn]
                    ON [rn].[OldId] = [oa].[Id]"
            );
        }

        private void RoutesTable_Int32ToGuid()
        {
            RenameTable("dbo.Routes", "OldRoutes");

            CreateTable(
                "dbo.Routes",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Fr8Account_Id = c.String(maxLength: 128),
                    Name = c.String(nullable: false),
                    Description = c.String(),
                    RouteState = c.Int(nullable: false),
                    Tag = c.String()
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.Id)
                .ForeignKey("dbo.Users", t => t.Fr8Account_Id)
                .ForeignKey("dbo._RouteStateTemplate", t => t.RouteState, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.Fr8Account_Id)
                .Index(t => t.RouteState);

            Sql(
                @"INSERT INTO [dbo].[Routes] (
                    [Id],
                    [Fr8Account_Id],
                    [Name],
                    [Description],
                    [RouteState],
                    [Tag])
                SELECT
                    [rn].[Id] as [Id],
                    [or].[Fr8Account_Id],
                    [or].[Name],
                    [or].[Description],
                    [or].[RouteState],
                    [or].[Tag]
                FROM [dbo].[OldRoutes] AS [or]
                INNER JOIN [dbo].[RouteNodes] [rn]
                    ON [rn].[OldId] = [or].[Id]"
            );
        }

        private void RoutesTable_GuidToInt32()
        {
            RenameTable("dbo.Routes", "OldRoutes");

            CreateTable(
                "dbo.Routes",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Fr8Account_Id = c.String(maxLength: 128),
                    Name = c.String(nullable: false),
                    Description = c.String(),
                    RouteState = c.Int(nullable: false),
                    Tag = c.String()
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.Id)
                .ForeignKey("dbo.Users", t => t.Fr8Account_Id)
                .ForeignKey("dbo._RouteStateTemplate", t => t.RouteState, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.Fr8Account_Id)
                .Index(t => t.RouteState);

            Sql(
                @"INSERT INTO [dbo].[Routes] (
                    [Id],
                    [Fr8Account_Id],
                    [Name],
                    [Description],
                    [RouteState],
                    [Tag])
                SELECT
                    [rn].[Id] as [Id],
                    [or].[Fr8Account_Id],
                    [or].[Name],
                    [or].[Description],
                    [or].[RouteState],
                    [or].[Tag]
                FROM [dbo].[OldRoutes] AS [or]
                INNER JOIN [dbo].[RouteNodes] [rn]
                    ON [rn].[OldId] = [or].[Id]"
            );
        }

        private void SubroutesTable_Int32ToGuid()
        {
            RenameTable("dbo.Subroutes", "OldSubroutes");

            CreateTable(
                "dbo.Subroutes",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    StartingSubroute = c.Boolean(nullable: false),
                    NodeTransitions = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.Id)
                .Index(t => t.Id);

            Sql(
                @"INSERT INTO [dbo].[Subroutes] (
                    [Id],
                    [Name],
                    [StartingSubroute],
                    [NodeTransitions])
                SELECT
                    [rn].[Id] as [Id],
                    [os].[Name],
                    [os].[StartingSubroute],
                    [os].[NodeTransitions]
                FROM [dbo].[OldSubroutes] AS [os]
                INNER JOIN [dbo].[RouteNodes] [rn]
                    ON [rn].[OldId] = [os].[Id]"
            );
        }

        private void SubroutesTable_GuidToInt32()
        {
            RenameTable("dbo.Subroutes", "OldSubroutes");

            CreateTable(
                "dbo.Subroutes",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                    StartingSubroute = c.Boolean(nullable: false),
                    NodeTransitions = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.Id)
                .Index(t => t.Id);

            Sql(
                @"INSERT INTO [dbo].[Subroutes] (
                    [Id],
                    [Name],
                    [StartingSubroute],
                    [NodeTransitions])
                SELECT
                    [rn].[Id] as [Id],
                    [os].[Name],
                    [os].[StartingSubroute],
                    [os].[NodeTransitions]
                FROM [dbo].[OldSubroutes] AS [os]
                INNER JOIN [dbo].[RouteNodes] [rn]
                    ON [rn].[OldId] = [os].[Id]"
            );
        }
    }
}
