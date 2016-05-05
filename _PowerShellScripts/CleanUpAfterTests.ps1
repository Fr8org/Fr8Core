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
	inner join #Nodes cp on cp.Id = derived.ObjectId

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


delete m from MTData m
	inner join [AspNetUsers] on AspNetUsers.Id = m.Fr8AccountId 
	where UserName = 'integration_test_runner@fr8.company' 

delete m from History m
	inner join [AspNetUsers] on AspNetUsers.Id = m.Fr8UserId 
	where UserName = 'integration_test_runner@fr8.company' 	
	
delete m from History m
	inner join #Nodes cp on  convert(nvarchar(50),cp.Id) = m.ObjectId

drop table #Nodes
"

Write-Host $commandText

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 300 #5 minutes

$command.ExecuteNonQuery()

Write-Host "Successfully removed test data."

$connection.Close()
