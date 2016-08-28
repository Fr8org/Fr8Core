<#
    .SYNOPSIS
	The script updates terminal hostnames in the database to prepare it for integration testing.
	Only Fr8 own terminals are affected. They should have port-based URLs (e.g. localhost:12345)
	and have IsFr8OwnTerminal set to 1.
	Called during CI processes for Dev/Master builds. 
#>
param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $true)]
	[ValidateSet("sta", "dev",  "prod", "staging","development", "production")]
	[string]$environment,

	[Parameter(Mandatory = $false)]
	[string]$newHostname,

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName,

	[Parameter(Mandatory = $false)]
	[string]$serviceName
)

if (-Not $newHostname.StartsWith("http://"))
{
	$newHostname = "http://{newHostName}"
}

$ErrorActionPreference = 'Stop'
$commandTextTmpl = "
	UPDATE Terminals SET [Endpoint] = 
	('{newHostname}' + RIGHT ([DevUrl], CHARINDEX (':', REVERSE ([DevUrl]))))
	WHERE CHARINDEX (':', REVERSE ([DevUrl])) <= 6 AND IsFr8OwnTerminal = 1"


switch ($environment) {
	dev {}
	development {
		if ($newHostname -eq $null) {
			throw "-newHostname is not specified. This argument is required for the development environment."
		}
		$commandText = $commandTextTmpl -replace '{newHostname}', $newHostname
		break;	
	}
	sta {}
	staging {
		if ($serviceName -eq $null) {
			throw "-serviceName is not specified. This argument is required for the staging environment."
		}
		$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
		if ($newHostname -ne $null) {
			Write-Warning "-newHostname parameter is ignored when -environment is set to 'staging'"
		}
		$newHostname = "http://" + $deployment.Url.Host
		Write-Host "Staging hostname is $newHostname"
		$commandText = $commandTextTmpl -replace '{newHostname}', $newHostname
		break;
	}
	prod {}
	production {
		if ($newHostname -ne $null) {
			Write-Warning "-newHostname parameter is ignored when -environment is set to 'production'"
		}

		$commandText = "
			UPDATE Terminals SET [Endpoint] = [ProdUrl] WHERE ProdUrl IS NOT NULL AND ParticipationState=1"
		break;
	}
}

Write-Host "Updating terminal URLs to $newHostname"

Write-Host $commandText 

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 20 #20 seconds
$command.ExecuteNonQuery()