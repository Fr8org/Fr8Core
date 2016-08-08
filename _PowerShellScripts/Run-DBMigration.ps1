param(
	[Parameter(Mandatory = $true)]
	[string]$migrationToolPath,
	
	[Parameter(Mandatory = $true)]
	[string]$newConnectionString,
	
	[Parameter(Mandatory = $false)]
	[string]$buildResultDirectory,
	
	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)
	

$RootDir = Split-Path -parent $PSCommandPath


$cfg = [xml](gc $RootDir\..\web.config)

$con= $cfg.configuration.connectionStrings.add|?{$_.name -eq "Fr8LocalDB"};
# Replace the content

if ([String]::IsNullOrEmpty($overrideDbName) -eq $false)
{
	$index = $newConnectionString.IndexOf("Initial Catalog=");
	if ($index -gt 0)
	{
		$lastIndex = $newConnectionString.IndexOf(';', $index);
	   
		if ($lastIndex -gt 0)
		{
			$suffix = $newConnectionString.Substring($lastIndex);
		}
		else
		{
			$suffix = ''
		}

		$newConnectionString = $newConnectionString.Substring(0, $index + "Initial Catalog=".Length) + $overrideDbName + $suffix;
	}
	else
	{
		$newConnectionString = $newConnectionString.TrimEnd(';') + ';Initial Catalog='+$overrideDbName+';'
	}
}

$con.connectionString = $newConnectionString

$cfg.Save("$RootDir\..\_Web.config");

$runMigrationCmd = "$RootDir\..\$migrationToolPath Data.dll /startupDirectory=`"$RootDir\..\$buildResultDirectory\bin`"  /startupConfigurationFile=`"$RootDir\..\$buildResultDirectory\_web.config`"";

Write-Host $runMigrationCmd

Invoke-Expression $runMigrationCmd

Remove-Item $RootDir\..\_Web.config