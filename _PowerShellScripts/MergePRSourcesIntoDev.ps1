<#
    .SYNOPSIS
    The script gets the latest dev branch, creates new branch from dev and merges feature branch into it.
	This script is intended to avoid situation when new pull requests breaks dev branch after merge. 
#>

param(
    [string]$sourceBranchName = $env:BUILD_SOURCEBRANCHNAME
)

$buildBranchName = "dev+$sourceBranchName"

Write-Host "Switching to dev branch..."
Invoke-Expression "git fetch"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to fetch branches."
	exit 1;
}

Invoke-Expression "git checkout dev"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to checkout dev branch."
	exit 1;
}

Write-Host "Getting the latest dev branch from GitHub repo..."
Invoke-Expression "git pull origin dev"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to get latest dev branch."
	exit 1;
}

Write-Host "Creating new branch $buildBranchName for build process..."
Invoke-Expression "git checkout -b $buildBranchName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to checkout new branch for build process."
	exit 1;
}

Write-Host "Merging $sourceBranchName into new branch: $buildBranchName"
Invoke-Expression "git merge $sourceBranchName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to merge dev into new branch."
	exit 1;
}






