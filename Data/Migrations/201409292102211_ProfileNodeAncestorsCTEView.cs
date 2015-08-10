namespace Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ProfileNodeAncestorsCTEView : DbMigration
    {
        public override void Up()
        {
            Sql(@"
CREATE VIEW [dbo].[ProfileNodeAncestorsCTEView]
AS
WITH cte AS ( 
	SELECT n0.[Id] as AnchorNodeID, n0.Id AS ProfileNodeID, n0.ParentNodeID AS ProfileParentNodeID
    FROM [dbo].[ProfileNodes] n0
		UNION ALL
    SELECT cte.AnchorNodeID, n1.Id, n1.ParentNodeID
    FROM cte
        INNER JOIN [dbo].[ProfileNodes] n1 ON n1.Id = cte.ProfileParentNodeID
    )
SELECT CONVERT(INT, ROW_NUMBER() OVER (ORDER BY ProfileNodeID desc)) as [Id], *
FROM    cte");

        }

        public override void Down()
        {
            Sql(@"DROP VIEW [dbo].[ProfileNodeAncestorsCTEView]");
        }
    }
}
