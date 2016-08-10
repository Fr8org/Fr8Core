namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_Process_To_Container_Migration : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Processes", newName: "Containers");
            RenameTable(name: "dbo._ProcessStateTemplate", newName: "_ContainerStateTemplate");
            RenameColumn(table: "dbo.ProcessNodes", name: "ParentProcessId", newName: "ParentContainerId");
            RenameColumn(table: "dbo.Containers", name: "ProcessState", newName: "ContainerState");
            RenameIndex(table: "dbo.Containers", name: "IX_ProcessState", newName: "IX_ContainerState");
            RenameIndex(table: "dbo.ProcessNodes", name: "IX_ParentProcessId", newName: "IX_ParentContainerId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ProcessNodes", name: "IX_ParentContainerId", newName: "IX_ParentProcessId");
            RenameIndex(table: "dbo.Containers", name: "IX_ContainerState", newName: "IX_ProcessState");
            RenameColumn(table: "dbo.Containers", name: "ContainerState", newName: "ProcessState");
            RenameColumn(table: "dbo.ProcessNodes", name: "ParentContainerId", newName: "ParentProcessId");
            RenameTable(name: "dbo._ContainerStateTemplate", newName: "_ProcessStateTemplate");
            RenameTable(name: "dbo.Containers", newName: "Processes");
        }
    }
}
