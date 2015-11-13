namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Container_Id_Int32_To_Guid : DbMigration
    {
        public override void Up()
        {
            System.Diagnostics.Debugger.Launch();

            DropForeignKey("dbo.ProcessNodes", "ParentProcessId", "dbo.Processes");
            DropIndex("dbo.ProcessNodes", new[] { "ParentContainerId" });

            DropPrimaryKey("dbo.Containers");
            RenameColumn("dbo.Containers", "Id", "OldId");
            AddColumn("dbo.Containers", "Id", c => c.Guid(nullable: true));
            Sql("UPDATE [dbo].[Containers] SET [Id] = newid()");
            AlterColumn("dbo.Containers", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.Containers", "Id");

            RenameColumn("dbo.ProcessNodes", "ParentContainerId", "OldParentContainerId");
            AddColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Guid(nullable: true));
            Sql("UPDATE [PN] SET [ParentContainerId] = [C].[Id] FROM [dbo].[ProcessNodes] AS [PN] INNER JOIN [dbo].[Containers] [C] ON [C].[OldId] = [PN].[OldParentContainerId]");
            AlterColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Guid(nullable: false));
            
            CreateIndex("dbo.ProcessNodes", "ParentContainerId");
            AddForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers", "Id");

            DropColumn("dbo.ProcessNodes", "OldParentContainerId");
            DropColumn("dbo.Containers", "OldId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers");
            DropIndex("dbo.ProcessNodes", new[] { "ParentContainerId" });

            DropPrimaryKey("dbo.Containers");
            RenameColumn("dbo.Containers", "Id", "OldId");
            AddColumn("dbo.Containers", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Containers", "Id");

            RenameColumn("dbo.ProcessNodes", "ParentContainerId", "OldParentContainerId");
            AddColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Int(nullable: true));
            Sql("UPDATE [PN] SET [ParentContainerId] = [C].[Id] FROM [dbo].[ProcessNodes] AS [PN] INNER JOIN [dbo].[Containers] [C] ON [C].[OldId] = [PN].[OldParentContainerId]");
            AlterColumn("dbo.ProcessNodes", "ParentContainerId", c => c.Int(nullable: false));

            CreateIndex("dbo.ProcessNodes", "ParentContainerId");
            AddForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers", "Id");

            DropColumn("dbo.ProcessNodes", "OldParentContainerId");
            DropColumn("dbo.Containers", "OldId");
        }
    }
}
