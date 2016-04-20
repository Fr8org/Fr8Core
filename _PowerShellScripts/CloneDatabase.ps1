param(
    [string]$connectionString,
	[string]$targetDbName,
	[string]$backupPath,
	[string]$dbPath
)

Write-Host "Deletes old target database if exists and creates a new one from the specified database."
$errorMessage = "An error while executing the query. Possibly cannot connect to the database to clone it. Please check connection string for CloneDatabase action."

$commandText = "
IF (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '[$($targetDbName)]' OR name = '[$($targetDbName)]')))
BEGIN
	ALTER DATABASE [$($targetDbName)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE [$($targetDbName)]
END"

Write-Host $commandText

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 300 #5 minutes

if ($command.ExecuteNonQuery() -ne -1)
{
	Write-Host $errorMessage
	exit 1
}
Write-Host "Successfully deleted old target database."


$commandText = "
IF OBJECT_ID('tempdb..#DirectoryTree') IS NOT NULL
DROP TABLE #DirectoryTree
   
IF OBJECT_ID('tempdb..#OrderedFiles') IS NOT NULL
   DROP TABLE #OrderedFiles

 CREATE TABLE #DirectoryTree (
               id int IDENTITY(1,1)
              ,subdirectory nvarchar(512)
              ,depth int
              ,isfile bit);

 INSERT #DirectoryTree (subdirectory, depth, isfile)  exec xp_dirtree '$($backupPath)', 1, 1

 select subdirectory as FileName, TRY_PARSE(LEFT(subdirectory, CHARINDEX('.', subdirectory) - 1) as int using 'en-US') as Ordering into #OrderedFiles from #DirectoryTree where isFile = 1 

 declare @mostRecentBackupOrder int;
 declare @backupToUse nvarchar(512);
 
 select @mostRecentBackupOrder = (select top(1) Ordering from #OrderedFiles order by Ordering desc)

 if @mostRecentBackupOrder is null
 	RAISERROR  ('No available backups were found', 20, -1) with log;
 
 select @backupToUse = (select top (1) FileName from #OrderedFiles where Ordering < @mostRecentBackupOrder order by Ordering desc)
 
 -- looks like we have only one backup available. Not good, but we still can continue working using the only available backup
 -- it is dangerous, because this file can currently be written by DB backup script, but we have no choise
 if @backupToUse is null
 begin
	select @backupToUse = (select top (1) FileName from #OrderedFiles where Ordering = @mostRecentBackupOrder)
 end

drop table #DirectoryTree
drop table #OrderedFiles

select @backupToUse = '$($backupPath)\' + @backupToUse;

RESTORE DATABASE [$($targetDbName)]
FROM DISK = @backupToUse
WITH MOVE 'DockyardDB2_data' TO '$($dbPath)\$($targetDbName).mdf',
MOVE 'DockyardDB2_log' TO '$($dbPath)\$($targetDbName).ldf'"

Write-Host $commandText
$command.CommandText = $commandText

$command.ExecuteNonQuery()

Write-Host "Successfully cloned the database."

$connection.Close()