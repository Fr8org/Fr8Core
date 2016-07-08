<#
    .SYNOPSIS
	Creates a Nuget package containing Terminal SDK components. Unlike the standard VSO action
	for packaging Nuget components, this script assigns a custom version number which 
	Nuget can recognize as "Prerelease".
#>

param(
    [Parameter(Mandatory = $false)]
	[string]$version = "1.0.2",

    [Parameter(Mandatory = $false)]
	[string]$suffix,

	[Parameter(Mandatory = $true)]
	[string]$buildConfiguration = "Dev"

)
$ErrorActionPreference = 'Stop'

$packageVersion = "$version.$env:BUILD_BUILDID"

if (-not [System.String]::IsNullOrEmpty($suffix)) {
	$packageVersion = $packageVersion + "-" + $suffix
}

Write-Host "Package version: $packageVersion"
$nuget = "{0}\agent\worker\tools\NuGet.exe pack {1}\{2} -OutputDirectory {1}\{3} -Properties Configuration=$buildConfiguration -IncludeReferencedProjects  -Version {4}" -f $env:AGENT_HOMEDIRECTORY, $env:BUILD_REPOSITORY_LOCALPATH, "Fr8TerminalBase.NET\Fr8TerminalSDK.$buildConfiguration.nuspec", "Fr8TerminalBase.NET", $packageVersion
Write-Host "Running Nuget packager: $nuget"
Invoke-Expression $nuget