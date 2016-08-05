param(
    [string]$connectionString,
	[string]$dbName
)

Write-Host "Removing all data created during integration testing."
$errorMessage = "An error while executing the query. Please check connection string for the DeleteDatabase action."

$commandText = "
if object_id('tempdb..#Nodes') is not null
    drop table #Nodes;

WITH NodeTree1 AS (
    SELECT seedNodes.Id
    FROM PlanNodes seedNodes
		inner join [AspNetUsers] on AspNetUsers.Id = seedNodes.Fr8AccountId 
	    where UserName = 'integration_test_runner@fr8.company'
    UNION ALL
    SELECT r.Id
    FROM PlanNodes r
    INNER JOIN NodeTree1 rt ON rt.Id = r.ParentPlanNodeId
)
select distinct (NodeTree1.Id) into #Nodes from NodeTree1 left join PlanNodes on NodeTree1.Id = PlanNodes.Id order by NodeTree1.Id;
 
update p set p.ParentPlanNodeId = null, p.RootPlanNodeId = null from PlanNodes p 
	inner join #Nodes cp on cp.Id = p.Id

-- list of ObjectRolePermissions
delete derived from ObjectRolePermissions derived 
	inner join #Nodes cp on convert(nvarchar(50), cp.Id) = derived.ObjectId
	where derived.[Type] <> 'Plan Template'

delete m from History m
	inner join Containers as derived on convert(nvarchar(50), derived.Id) = m.ObjectId
	inner join #Nodes cp on cp.Id = derived.PlanId
	
-- list of Containers
delete derived from Containers derived 
	inner join #Nodes cp on cp.Id = derived.PlanId

-- list of Plans
delete derived from Plans derived 
	inner join #Nodes cp on cp.Id = derived.Id

-- list of SubPlans
delete derived from SubPlans derived 
	inner join #Nodes cp on cp.Id = derived.Id

-- list of Actions
delete derived from Actions derived 
	inner join #Nodes cp on cp.Id = derived.Id


-- list of PlanNodes
delete derived from PlanNodes derived 
	inner join #Nodes cp on cp.Id = derived.Id

delete orp from ObjectRolePermissions orp
	inner join [MTData] m on orp.[Type] = 'Plan Template' and convert(nvarchar(50), m.Id) = orp.ObjectId
	inner join [AspNetUsers] on AspNetUsers.Id = m.Fr8AccountId 
	where UserName = 'integration_test_runner@fr8.company'

delete m from MTData m
	inner join [AspNetUsers] on AspNetUsers.Id = m.Fr8AccountId 
	where UserName = 'integration_test_runner@fr8.company' 

delete m from History m
	inner join [AspNetUsers] on AspNetUsers.Id = m.Fr8UserId 
	where UserName = 'integration_test_runner@fr8.company' 	
	
delete m from History m
	inner join #Nodes cp on  convert(nvarchar(50),cp.Id) = m.ObjectId

drop table #Nodes

-- delete account used in register
IF object_id('tempdb..#TempUsers') IS NOT NULL
    DROP TABLE #TempUsers;

WITH TestUser (UserId)
AS
(
	SELECT Users.Id 
	FROM Users
	WHERE Users.EmailAddressID IN (	
		SELECT EmailAddresses.Id
		FROM EmailAddresses
		WHERE EmailAddresses.Address LIKE 'testuser_%@fr8.co')
)
SELECT DISTINCT (TestUser.UserId) INTO #TempUsers FROM TestUser

IF object_id('tempdb..#Nodes2') IS NOT NULL
	DROP TABLE #Nodes2;

WITH NodeTree AS (
    SELECT seedNodes.Id
    FROM PlanNodes seedNodes INNER JOIN #TempUsers tu ON tu.UserId = seedNodes.Fr8AccountId
)
SELECT DISTINCT (NodeTree.Id) INTO #Nodes2 FROM NodeTree

UPDATE derived
SET derived.ParentPlanNodeId = null, derived.RootPlanNodeId = null
FROM PlanNodes derived INNER JOIN #Nodes2 n on n.Id = derived.Id

DELETE derived FROM ObjectRolePermissions derived INNER JOIN #Nodes2 n on n.Id = derived.ObjectId
WHERE derived.[Type] <> 'Plan Template'

DELETE h FROM History h 
INNER JOIN Containers AS derived ON CONVERT(NVARCHAR(50), derived.Id) = h.ObjectId
INNER JOIN #Nodes2 n ON n.Id = derived.PlanId

DELETE derived FROM Containers derived INNER JOIN #Nodes2 n ON n.Id = derived.PlanId

DELETE derived FROM Plans derived INNER JOIN #Nodes2 n ON n.Id = derived.Id

DELETE derived FROM SubPlans derived INNER JOIN #Nodes2 n ON n.Id = derived.Id

DELETE derived FROM Actions derived INNER JOIN #Nodes2 n ON n.Id = derived.Id

DELETE derived FROM PlanNodes derived INNER JOIN #Nodes2 n ON n.Id = derived.Id

DELETE derived FROM ObjectRolePermissions derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Fr8AccountId

DELETE derived FROM MTData derived INNER JOIN #TempUsers tu ON tu.UserId = derived.fr8AccountId

DELETE derived FROM History derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Fr8UserId OR tu.UserId = derived.CreatedByID

DELETE derived FROM AuthorizationTokens derived INNER JOIN #TempUsers tu ON tu.UserId = derived.UserID

DELETE derived FROM AspNetUserRoles derived INNER JOIN #TempUsers tu ON tu.UserId = derived.UserId

DELETE derived FROM Users derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Id

DELETE FROM EmailAddresses WHERE EmailAddresses.Address LIKE 'testuser_%@fr8.co'

DELETE derived FROM AspNetUsers derived INNER JOIN #TempUsers tu ON tu.UserId = derived.Id

DROP TABLE #Nodes2;
DROP TABLE #TempUsers;
"

Write-Host $commandText

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 300 #5 minutes

$command.ExecuteNonQuery()

Write-Host "Successfully removed test data."

$connection.Close()
