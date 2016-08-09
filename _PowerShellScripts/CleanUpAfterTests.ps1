param(
    [string]$connectionString,
	[string]$dbName
)

$errorMessage = "An error while executing the query. Please check connection string for the DeleteDatabase action."

Write-Host "Removing all data created during integration testing."
$commandText = "
IF object_id('tempdb..#Nodes') IS NOT NULL
	DROP TABLE #Nodes;

WITH NodeTree1 AS (
	SELECT seedNodes.Id
	FROM PlanNodes seedNodes INNER JOIN [AspNetUsers] ON AspNetUsers.Id = seedNodes.Fr8AccountId 
	WHERE UserName = 'integration_test_runner@fr8.company'
	UNION ALL
	SELECT r.Id
	FROM PlanNodes r
	INNER JOIN NodeTree1 rt ON rt.Id = r.ParentPlanNodeId
)
SELECT DISTINCT (NodeTree1.Id) INTO #Nodes FROM NodeTree1 LEFT JOIN PlanNodes ON NodeTree1.Id = PlanNodes.Id ORDER BY NodeTree1.Id;
 
UPDATE p SET p.ParentPlanNodeId = null, p.RootPlanNodeId = null FROM PlanNodes p INNER JOIN #Nodes cp ON cp.Id = p.Id;

-- List of ObjectRolePermissions
DELETE derived FROM ObjectRolePermissions derived
INNER JOIN #Nodes cp ON CONVERT(nvarchar(50), cp.Id) = derived.ObjectId
WHERE derived.[Type] <> 'Plan Template';

-- List of History
DELETE m FROM History m
INNER JOIN Containers as derived ON CONVERT(nvarchar(50), derived.Id) = m.ObjectId
INNER JOIN #Nodes cp ON cp.Id = derived.PlanId;
	
-- List of Containers
DELETE derived FROM Containers derived INNER JOIN #Nodes cp ON cp.Id = derived.PlanId;

-- List of Plans
DELETE derived FROM Plans derived INNER JOIN #Nodes cp ON cp.Id = derived.Id;

-- List of SubPlans 
DELETE derived FROM SubPlans derived INNER JOIN #Nodes cp ON cp.Id = derived.Id;

-- List of Actions
DELETE derived FROM Actions derived INNER JOIN #Nodes cp ON cp.Id = derived.Id;

-- List of PlanNodes
DELETE derived FROM PlanNodes derived INNER JOIN #Nodes cp ON cp.Id = derived.Id;

DELETE orp FROM ObjectRolePermissions orp 
INNER JOIN [MTData] m ON orp.[Type] = 'Plan Template' AND CONVERT(nvarchar(50), m.Id) = orp.ObjectId
INNER JOIN [AspNetUsers] ON AspNetUsers.Id = m.Fr8AccountId 
WHERE UserName = 'integration_test_runner@fr8.company';

DELETE m FROM MTData m INNER JOIN [AspNetUsers] ON AspNetUsers.Id = m.Fr8AccountId WHERE UserName = 'integration_test_runner@fr8.company';

DELETE m FROM History m INNER JOIN [AspNetUsers] ON AspNetUsers.Id = m.Fr8UserId WHERE UserName = 'integration_test_runner@fr8.company';
	
DELETE m FROM History m INNER JOIN #Nodes cp ON CONVERT(nvarchar(50),cp.Id) = m.ObjectId;

DROP TABLE #Nodes;"

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$connection.Open()

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$command.CommandTimeout = 300 #5 minutes
$command.ExecuteNonQuery()

Write-Host "Removing all data created during registration testing."
$commandText = ""

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$command.CommandTimeout = 300 #5 minutes
$command.ExecuteNonQuery()

Write-Host "Successfully removed test data."

$connection.Close()
