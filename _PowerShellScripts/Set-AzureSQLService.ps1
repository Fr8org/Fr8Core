<#
The script updates service objective for the specified database). 
Service objective determines throughput and max size of the database and thus cost per month.
#>

param(
     [string]$Location = "West US",
	 [string]$ServerName = "s79ifqsqga",
	 [Parameter(Mandatory = $true)]
     [string]$DbName,
     [string]$ServiceObjective = "S1"
)

$ErrorActionPreference = 'Stop'

$db = Get-AzureSqlDatabase -ServerName $ServerName –DatabaseName $dbName
$obj = Get-AzureSqlDatabaseServiceObjective $ServerName -ServiceObjectiveName $ServiceObjective
Set-AzureSqlDatabase  –Database $db -ServerName $ServerName –ServiceObjective $obj -Force