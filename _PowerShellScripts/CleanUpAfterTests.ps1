param(
    [string]$connectionString,
	[string]$dbName
)

Write-Host "Removing all data created during integration testing."
$errorMessage = "An error while executing the query. Please check connection string for the DeleteDatabase action."

$commandText = "
if object_id('tempdb..#Nodes') is not null
    drop table #Nodes

select cp.Id into #Nodes from Plans p 
	inner join PlanNodes pn on p.Id = pn.Id  
	inner join [AspNetUsers] on AspNetUsers.Id = pn.Fr8AccountId 
	inner join PlanNodes cp on cp.RootPlanNodeId = p.Id
    where UserName = 'integration_test_runner@fr8.company' 

update p set p.ParentPlanNodeId = null, p.RootPlanNodeId = null from PlanNodes p 
	inner join #Nodes cp on cp.Id = p.Id

-- list of ObjectRolePrivileges
delete derived from ObjectRolePrivileges derived 
	inner join #Nodes cp on cp.Id = derived.ObjectId

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