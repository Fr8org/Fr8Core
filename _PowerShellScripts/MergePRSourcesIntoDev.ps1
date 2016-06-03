<#
    .SYNOPSIS
    The script gets the latest dev branch, creates new branch from dev and merges feature branch into it.
	This script is intended to avoid situation when new pull requests breaks dev branch after merge. 
#>

param(
	[Parameter(Mandatory = $true)]
	[string]$github_username,

	[Parameter(Mandatory = $true)]
	[string]$github_password,

	[string]$mainBranchName = "dev",
    [string]$sourceBranchName = $env:BUILD_SOURCEBRANCHNAME,
    [string]$tempDirectory = $env:BUILD_STAGINGDIRECTORY
)

Function DeleteBranchIfExists($buildBranchName)
{
	$command = "git branch --list $buildBranchName"
	$result = Invoke-Expression $command

	if (![System.String]::IsNullOrEmpty($result))
	{
		$command = "git branch -D $buildBranchName"
		Invoke-Expression $command    
	}

}

$tempFileName = $tempDirectory + "\gitCommandsOutput+$sourceBranchName.txt"

$giturl = "https://{0}:{1}@github.com/alexed1/fr8company" -f $github_username, $github_password

$buildBranchName = "$mainBranchName+$sourceBranchName"

Invoke-Expression "git fetch $giturl 2> $tempFileName"

if ($LastExitCode -ne 0)
{
	Write-Host "Failed to fetch branches from your repository."
    exit 1;
}

Write-Host "Switching to $mainBranchName branch..."
Invoke-Expression "git checkout $mainBranchName 2> $tempFileName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to checkout $mainBranchName branch."
	exit 1;
}

Write-Host "Getting the latest $mainBranchName branch from GitHub repo..."
Invoke-Expression "git pull $giturl $mainBranchName 2> $tempFileName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to get the latest $mainBranchName branch."
	exit 1;
}

DeleteBranchIfExists $buildBranchName

Write-Host "Creating new branch $buildBranchName for build process..."
Invoke-Expression "git checkout -b $buildBranchName 2> $tempFileName"
if ($LastExitCode -ne 0)
{
    Write-Host "Failed to create new branch for build process."
	exit 1;
}

Write-Host "Merging $sourceBranchName into new branch $buildBranchName"
Invoke-Expression "git merge origin/$sourceBranchName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to merge new branch $sourceBranchName into $mainBranchName. Please, make sure that branch $sourceBranchName has the latest sources from the $mainBranchName branch."
	exit 1;
}

