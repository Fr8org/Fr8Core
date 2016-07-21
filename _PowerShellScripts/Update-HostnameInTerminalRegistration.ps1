param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $true)]
	[string]$newHostname,

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)

Write-Host "Update terminal URLs to $newHostname"

$commandText = "
	-- Update hostname only if port value is present in endpoint URL and terminal belongs to Fr8
    UPDATE TerminalRegistration SET [Endpoint] = 
			('$newHostname' + RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint]))))
	WHERE CHARINDEX (':', REVERSE ([Endpoint])) <= 6 AND IsFr8OwnTerminal = 1
";

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
Write-Host "Successfully updated terminal hostname."

$connection.Close()