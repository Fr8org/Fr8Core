<#
    .SYNOPSIS
    The script changes application settings and connection strings in the provided web.config or app.config file. 
	To update an application setting, prefix its name with as: and in order to update a connection string, prefix its 
	name with cs:

	The script can only update one file at a time. 

	If setting value contains the token {staurl}, it will be replaced with Cloud Service staging endpoint schema and hostname.
	This capability requires that the -serviceName argument is specified and the Azure context is present 
	(so use the Azure PowerShell action in VSO to add the script). 
	
	.EXAMPLE
	.\Set-Config.ps1 -as:HubApiBaseUrl "http://localhost" -as:AzureSearchApiKey "IDKLSWOVC" -cs:Fr8LocalDB "Data Source=.;Initial Catalog=Fr8LocalDB;Integrated Security=SSPI;Transaction Binding=Explicit Unbind;"	
#>

$connectionStrings = @{}
$overrideDbName = @{}
$appSettings = @{}
$filePath = $null
$overrideDbName = $null
$serviceName = $null

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

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-serviceName")) {
		$serviceName = $args[$i]
	}
}

$ErrorActionPreference = 'Stop'

# In case service name is provided, the cloud service staging endpoint hostname is expected 
# to be assigned to some configuration setting
if ($serviceName -ne $null)
{
	$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
	$stagingHostname = $deployment.Url.Host

	if ($stagingHostname -eq $null)
	{
		Write-Error "Unable to get Staging slot URL for Cloud Service $serviceName"
		exit 1
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

	# Get appSettigns section
	$settings = $xml.appSettings.add # if an incuded settings file
	if (($settings -eq $null) -or ($settings.Count -eq 0))
	{
		$settings = $xml.configuration.appSettings.add # if a web/app.config
	}

	ForEach($item in $appSettings.GetEnumerator())
	{
		Write-Host ("Updating {0} value" -f $item.Name) 
		$node = $settings | where {$_.key -eq $item.Name}
		if ($node -ne $NULL)
		{
			# Handle reference to Staging Cloud Service URL 
			if ($item.Value.Contains("{staurl}"))
			{
				# Assign Staging endpoint hostname to the setting
				if ($serviceName -ne $null)
				{
					$node.Value = $item.Value -replace "{staurl}", $stagingHostname 
				}
				else 
				{
					Write-Error "-serviceName argument must be specified in order to set staging endpoint URL ('staurl:')"
					exit 1
				}
			}
			else 
			{
				$node.Value = $item.Value
			}
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

