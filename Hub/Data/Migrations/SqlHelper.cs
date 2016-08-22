namespace Data.Migrations
{
	public static class SqlHelper
	{
		public static string DropDefaultConstraint(string table, string column)
		{
			return string.Format(@"
				DECLARE @name sysname

				SELECT @name = dc.name
				FROM sys.columns c
				JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
				WHERE c.object_id = OBJECT_ID('{0}')
				AND c.name = '{1}'

				IF @name IS NOT NULL EXECUTE ('ALTER TABLE {0} DROP CONSTRAINT ' + @name)
				", 
				table, column);
		}
	}
}