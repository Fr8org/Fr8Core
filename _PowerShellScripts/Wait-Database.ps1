param(
	[Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName,

	[Parameter(Mandatory = $true)]
	[int]$timeout
)

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$startTime = [DateTime]::UtcNow
$commandText = "INSERT INTO Logs (Message, LastUpdated, CreateDate) VALUES ('Waiting for DB availability...', GetUtcDate(), GetUtcDate()) DELETE FROM Logs WHERE Id=@@IDENTITY;"
while ($true) {
	try {
		Write-Host "Attempt to connect to the database..."
		$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
		$connection.Open()
		$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
		$command.CommandTimeout = 10 #10 seconds
		$command.ExecuteNonQuery() | Out-Null
		exit 0
	}
	catch {
		if ([DateTime]::UtcNow -ge $startTime.AddSeconds($timeout) ) {
			Write-Host "Database availability timeout."
			exit 1
		}
	}
	finally {
		if ($connection.State -eq [System.Data.ConnectionState]::Open) {
			$connection.Close()
		}		
	}
	Start-Sleep -s 20
}

