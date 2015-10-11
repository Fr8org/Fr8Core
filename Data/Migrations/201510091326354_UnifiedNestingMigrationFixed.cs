using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnifiedNestingMigrationFixed : DbMigration
    {
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public override void Up()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ParentTemplateId" });
            DropPrimaryKey("dbo.ProcessNodeTemplates");
            DropPrimaryKey("dbo.ProcessTemplates");
            AlterColumn("dbo.ProcessNodeTemplates", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.ProcessTemplates", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.ProcessNodeTemplates", "Id");
            AddPrimaryKey("dbo.ProcessTemplates", "Id");
            CreateIndex("dbo.ProcessTemplates", "Id");
            CreateIndex("dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates", "Id");
            AddForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
            DropColumn("dbo.ProcessNodeTemplates", "ParentTemplateId");
            DropColumn("dbo.ProcessNodeTemplates", "LastUpdated");
            DropColumn("dbo.ProcessNodeTemplates", "CreateDate");
            DropColumn("dbo.ProcessTemplates", "ProcessNodeTemplateOrdering");
            DropColumn("dbo.ProcessTemplates", "LastUpdated");
            DropColumn("dbo.ProcessTemplates", "CreateDate");

            var script = ReadSqlScript("201510091326354_UnifiedNestingMigrationFixed_up.sql");
            
            foreach (var stat in ParseScript(script))
            {
                Sql(stat);
            }
        }

        /**********************************************************************************/

        public override void Down()
        {
            var script = ReadSqlScript("201510091326354_UnifiedNestingMigrationFixed_down.sql");

            foreach (var stat in ParseScript(script))
            {
                Sql(stat);
            }

            AddColumn("dbo.ProcessTemplates", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessTemplates", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessTemplates", "ProcessNodeTemplateOrdering", c => c.String());
            AddColumn("dbo.ProcessNodeTemplates", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessNodeTemplates", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessNodeTemplates", "ParentTemplateId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Id" });
            DropIndex("dbo.ProcessTemplates", new[] { "Id" });
            DropPrimaryKey("dbo.ProcessTemplates");
            DropPrimaryKey("dbo.ProcessNodeTemplates");
            AlterColumn("dbo.ProcessTemplates", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ProcessNodeTemplates", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.ProcessTemplates", "Id");
            AddPrimaryKey("dbo.ProcessNodeTemplates", "Id");
            CreateIndex("dbo.ProcessNodeTemplates", "ParentTemplateId");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
        }
        
        /**********************************************************************************/

        private static string ReadSqlScript(string name)
        {
            var fileName = "Data.Migrations.SqlScripts." + name;
            var currentAssembly = Assembly.GetExecutingAssembly();

            using (var stream = currentAssembly.GetManifestResourceStream(fileName))
            {
                if (stream == null)
                {
                    throw new Exception(string.Format("SQL script {0} was not found in embeded resources for assembly {1}", fileName, currentAssembly.FullName));
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /**********************************************************************************/

        private static IEnumerable<string> ParseScript(string name)
        {
            List<string> script = new List<string>();

            using (var reader = new StringReader(name))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    
                    if (line == null || line == "GO")
                    {
                        if (script.Count > 0)
                        {
                            yield return string.Join("\n", script);
                        }

                        if (line == null)
                        {
                            break;
                        }

                        script.Clear();
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line) 
                        || line.StartsWith("--") 
                        || line=="COMMIT" 
                        || line.StartsWith("select Has_Perms_By_Name"))
                    {
                        continue;
                    }

                    script.Add(line);
                }
            }
        }
    }
}
