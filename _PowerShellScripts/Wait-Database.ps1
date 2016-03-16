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

while ($true) {
	try {
		Write-Host "Attempt to connect to the database..."
		$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
		$connection.Open()
		exit 0
	}
	catch {
		if ($_.Exception.Message -contains "is not currently available") {
			if ([DateTime]::UtcNow -ge $startTime.AddSeconds($timeout) ) {
				Write-Host "Database availability timeout."
				exit 1
			}
		}
		else {
			Write-Host "Error while connecting to the database:"  $_.Exception.Message
			exit 1
		}
	}
	Start-Sleep -s 5
}

