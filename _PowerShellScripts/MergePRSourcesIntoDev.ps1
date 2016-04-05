<#
    .SYNOPSIS
    The script gets the latest dev branch, creates new branch from dev and merges feature branch into it.
	This script is intended to avoid situation when new pull requests breaks dev branch after merge. 
#>

param(
    [string]$sourceBranchName = $env:BUILD_SOURCEBRANCHNAME,
    [string]$tempDirectory = $env:BUILD_STAGINGDIRECTORY
)

$tempFileName = $tempDirectory + "\gitCommandsOutput.txt"

if (Test-Path $tempFileName) {
  Remove-Item $tempFileName
}

$github_username = "fr8admin"
$github_password = "ulysses3"

$giturl = "https://{0}:{1}@github.com/alexed1/fr8company" -f $github_username, $github_password

$buildBranchName = "dev+$sourceBranchName"

Invoke-Expression "git fetch $giturl 2> $tempFileName"

if ($LastExitCode -ne 0)
{
	Write-Error "Failed to fetch branches from your repository."
    exit 1;
}

Write-Host "Switching to dev branch..."
Invoke-Expression "git checkout dev 2> $tempFileName"
if ($LastExitCode -ne 0)
{
	Write-Error "Failed to checkout dev branch."
	exit 1;
}

Write-Host "Getting the latest dev branch from GitHub repo..."
Invoke-Expression "git pull $giturl dev 2> $tempFileName"
if ($LastExitCode -ne 0)
{
	Write-Error "Failed to get the latest dev branch."
	exit 1;
}

$command = "git branch --list $buildBranchName"
$result = Invoke-Expression $command

if (![System.String]::IsNullOrEmpty($result))
{
    $command = "git branch -D $buildBranchName"
    Invoke-Expression $command    
}

Write-Host "Creating new branch $buildBranchName for build process..."
Invoke-Expression "git checkout -b $buildBranchName 2> $tempFileName"
if ($LastExitCode -ne 0)
{
    Write-Error "Failed to create new branch for build process."
	exit 1;
}

Write-Host "Merging $sourceBranchName into new branch $buildBranchName"
Invoke-Expression "git merge origin/$sourceBranchName"
if ($LastExitCode -ne 0)
{
	Write-Error "Failed to merge new branch $buildBranchName into dev. Please, make sure that branch $sourceBranchName has the latest sources from the dev branch."
	exit 1;
}

