param(
    [string]$connectionString,
	[string]$dbName
)

$errorMessage = "An error while executing the query. Please check connection string for the DeleteDatabase action."

Write-Host "Removing all data created during integration testing."
$commandText = "
IF object_id('tempdb..#Nodes') IS NOT NULL
	DROP TABLE #Nodes;

DECLARE @integration_email VARCHAR(50);
SET @integration_email = 'integration_test_runner@fr8.company';
	
WITH NodeTree1 AS (
	SELECT seedNodes.Id
	FROM PlanNodes seedNodes INNER JOIN [AspNetUsers] ON AspNetUsers.Id = seedNodes.Fr8AccountId 
	WHERE UserName = @integration_email
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
WHERE UserName = @integration_email;

DELETE m FROM MTData m INNER JOIN [AspNetUsers] ON AspNetUsers.Id = m.Fr8AccountId WHERE UserName = @integration_email;

DELETE m FROM History m INNER JOIN [AspNetUsers] ON AspNetUsers.Id = m.Fr8UserId WHERE UserName = @integration_email;
	
DELETE m FROM History m INNER JOIN #Nodes cp ON CONVERT(nvarchar(50),cp.Id) = m.ObjectId;

DROP TABLE #Nodes;"

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$connection.Open()

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$command.CommandTimeout = 300 #5 minutes
$command.ExecuteNonQuery()

Write-Host "Removing all data created during registration testing."
$commandText = "
IF object_id('tempdb..#TempUsers') IS NOT NULL
	DROP TABLE #TempUsers;

IF object_id('tempdb..#Nodes2') IS NOT NULL
	DROP TABLE #Nodes2;

DECLARE @registration_email VARCHAR(50);
SET @registration_email = 'testuser_registration@fr8.co';

WITH TestUser (UserId) AS (
	SELECT Users.Id 
	FROM Users
	WHERE Users.EmailAddressID IN (	
		SELECT EmailAddresses.Id
		FROM EmailAddresses
		WHERE EmailAddresses.Address LIKE @registration_email)
)
SELECT DISTINCT (TestUser.UserId) INTO #TempUsers FROM TestUser;

WITH NodeTree AS (
	SELECT seedNodes.Id
	FROM PlanNodes seedNodes INNER JOIN #TempUsers tu ON tu.UserId = seedNodes.Fr8AccountId
)
SELECT DISTINCT (NodeTree.Id) INTO #Nodes2 FROM NodeTree;

UPDATE derived SET derived.ParentPlanNodeId = null, derived.RootPlanNodeId = null FROM PlanNodes derived INNER JOIN #Nodes2 n ON n.Id = derived.Id;

DELETE derived FROM ObjectRolePermissions derived
INNER JOIN #Nodes2 n ON CONVERT(nvarchar(50), n.Id) = derived.ObjectId
WHERE derived.[Type] <> 'Plan Template';

DELETE h FROM History h 
INNER JOIN Containers AS derived ON CONVERT(nvarchar(50), derived.Id) = h.ObjectId
INNER JOIN #Nodes2 n ON n.Id = derived.PlanId;

DELETE derived FROM Containers derived INNER JOIN #Nodes2 n ON n.Id = derived.PlanId;

DELETE derived FROM Plans derived INNER JOIN #Nodes2 n ON n.Id = derived.Id;

DELETE derived FROM SubPlans derived INNER JOIN #Nodes2 n ON n.Id = derived.Id;

DELETE derived FROM Actions derived INNER JOIN #Nodes2 n ON n.Id = derived.Id;

DELETE derived FROM PlanNodes derived INNER JOIN #Nodes2 n ON n.Id = derived.Id;

DELETE derived FROM ObjectRolePermissions derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Fr8AccountId;

DELETE derived FROM MTData derived INNER JOIN #TempUsers tu ON tu.UserId = derived.fr8AccountId;

DELETE derived FROM History derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Fr8UserId OR tu.UserId = derived.CreatedByID;

DELETE derived FROM AuthorizationTokens derived INNER JOIN #TempUsers tu ON tu.UserId = derived.UserID;

DELETE derived FROM AspNetUserRoles derived INNER JOIN #TempUsers tu ON tu.UserId = derived.UserId;

DELETE derived FROM Users derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Id;

DELETE derived FROM Recipients derived WHERE derived.EmailID IN (
	SELECT derived.Id
	FROM Emails derived INNER JOIN EmailAddresses ON EmailAddresses.Id = derived.FromID
	WHERE EmailAddresses.Address LIKE @registration_email);

DELETE derived FROM Emails derived INNER JOIN EmailAddresses ON EmailAddresses.Id = derived.FromID WHERE EmailAddresses.Address LIKE @registration_email;

DELETE FROM EmailAddresses WHERE EmailAddresses.Address LIKE @registration_email;

DELETE derived FROM AspNetUsers derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Id;

DROP TABLE #Nodes2;
DROP TABLE #TempUsers;"

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$command.CommandTimeout = 300 #5 minutes
$command.ExecuteNonQuery()

Write-Host "Successfully removed test data."

$connection.Close()
