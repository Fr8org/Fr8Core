namespace Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	
	public partial class MERGE_Migration : System.Data.Entity.Migrations.DbMigration
	{
		public override void Up()
		{
			// http://stackoverflow.com/questions/17921886/update-database-fails-due-to-pending-changes-but-add-migration-creates-a-duplic

			// Intentionally left blank.

			// This may seem like a hack, but it is necessary when using source control.
			// When a migration is created via add-migration, EF creates 
			// an .edmx file from the current code first classes. It compares this .edmx to the .edmx stored in the last migration before this, 
			// which I'll call it's parent migration. The edmx snapshots are gzipped and stored in base64 in the resource files (.resx) if you 
			// want to see them. EF uses the difference between these two snapshots to determine what needs to be migrated.

			// When using source control it will happen that two users add entities to the model independently. The generated edmx snapshots will 
			// only have the changes that they have made. When they merge in source control, they will end up with this:

			// Migration                        |  Snapshot Contents
			// -------------------------------- | ----------------
			// 20150101_Parent Migration        |  A
			// 20150102_Developer 1's Migration |  A + Change 1
			// 20150103_Developer 2's Migration |  A + Change 2

			// So calling add-migration will create the current snapshot edmx from the Code First model and compare it to the 
			// the latest migration's snapshot, which is A + Change 2, and see that Change 1 is missing. That is why it 
			// creates a duplicate migration. We know that the migrations have already been applied, so the only thing that this 
			// migration will do is update the current snapshot .edmx so that later migrations work fine.
		}
		
		public override void Down()
		{

		}
	}
}
