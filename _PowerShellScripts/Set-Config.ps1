$connectionStrings = @{}
$overrideDbName = @{}
$appSettings = @{}
$filePath = $null
$overrideDbName = $null

For  ($i=0; $i -lt $args.Count; $i++) {
	if (($i -gt 0) -and ($args[$i - 1] -ieq "-cs:")) {
		$connectionStrings.Add($args[$i], $args[$i + 1])
	}

	if (($i -gt 0) -and ($args[$i - 1] -ieq "-as:")) {
		$appSettings.Add($args[$i], $args[$i + 1])
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-filePath")) {
		$filePath = $args[$i]
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-overrideDbName")) {
		$overrideDbName = $args[$i]
	}
}

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$configPath = [System.IO.Path]::Combine($rootDir, $filePath)

if(Test-Path $configPath)
{  
	Write-Host "Configuration file: $configPath" 
	$xml = [xml](Get-Content $configPath)

	ForEach($item in $connectionStrings.GetEnumerator())
	{
		Write-Host ("Updating {0} value" -f $item.Name)
		$node = $xml.configuration.connectionStrings.add | where {$_.name -eq $item.Name}
		if ($node -ne $NULL)
		{
            if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
		        $builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($item.Value)
		        $builder["Initial Catalog"] = $overrideDbName
		        $node.connectionString = $builder.ToString()
	        }
            else {
			    $node.connectionString = $item.Value
            }
		}
	}

	ForEach($item in $appSettings.GetEnumerator())
	{
		Write-Host ("Updating {0} value" -f $item.Name) 
		$node = $xml.configuration.appSettings.add | where {$_.key -eq $item.Name}
		if ($node -ne $NULL)
		{
			$node.Value = $item.Value
		}
	}

	try
	{
		$xml.Save($configPath)
		Write-Host "File updated."
	}
	catch
	{
		Write-Host "Exception while saving $($configPath) - $($_.Exception.Message)" 
		exit 1 
	}   
}
else
{
    Write-Error "Configuration file $($configPath) wasn't found."
}

