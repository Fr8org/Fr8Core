#
# MergeDevInsidePRSources.ps1
#

param(
    [string]$sourceBranchName = $env:BUILD_SOURCEBRANCHNAME,
	[string]$sourceDirectory = $env:BUILD_SOURCESDIRECTORY
)

$devBranchRef = "refs/heads/dev"
$buildBranchName = "dev+$sourceBranchName"

Write-Host "Switching to dev branch..."
Invoke-Expression "git fetch dev"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to fetch dev branch."
	return 1;
}

Invoke-Expression "git checkout dev"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to checkout dev branch."
	return 1;
}

Write-Host "Getting the latest dev branch from GitHub repo..."
Invoke-Expression "git pull dev"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to get latest dev branch."
	return 1;
}

Write-Host "Creating new branch $buildBranchName for build process..."
Invoke-Expression "git checkout -b $buildBranchName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to checkout new branch fro build process."
	return 1;
}

Write-Host "Merging dev into new branch: $buildBranchName"
Invoke-Expression "git merge $env:BUILD_SOURCEBRANCHNAME"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed to merge dev into new branch."
	return 1;
}






