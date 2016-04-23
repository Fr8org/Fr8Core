<#
    .SYNOPSIS
    The script updates the status of build on GitHub to disable Merge button if something wrong happened
    This script is intended to avoid situation when new pull requests breaks dev branch after merge. 
#>

param(
	[Parameter(Mandatory = $true)]
	[string]$vso_username,
	
	[Parameter(Mandatory = $true)]
	[string]$vso_password,

	[Parameter(Mandatory = $true)]
	[string]$github_username,

	[Parameter(Mandatory = $true)]
	[string]$github_password,

	[string]$mainBranchName = "dev",
    [string]$buildId = $env:BUILD_BUILDID,
	[string]$branchName = $env:BUILD_SOURCEBRANCHNAME,
	[string]$tempDirectory = $env:BUILD_STAGINGDIRECTORY
)

$tempFileName = $tempDirectory + "\gitCommandsOutput+$branchName.txt"
$target_url = "https://fr8.visualstudio.com/DefaultCollection/fr8/_build?_a=summary&buildId=" + $buildId

$failure = @{
				state = "failure"
				target_url = $target_url
				description = "The build failed"
				context = "feature-branch-ci/vso"
			} | ConvertTo-Json

$success = @{
				state = "success"
				target_url = $target_url
				description = "The build succeeded!"
				context = "feature-branch-ci/vso"
			} | ConvertTo-Json

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

Function UpdateGitHubBuildStatus($message)
{
	$sourceVersion = Invoke-Expression "git rev-parse origin/$branchName" | Out-String
	$sourceVersion = $sourceVersion.Trim()

	if ($LastExitCode -ne 0)
	{
		Write-Host "Failed to get latest commit hash."
        Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
        Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription
		
        exit 1;
	}
	else
	{
        Write-Host "Latest commit for branch $branchName is $sourceVersion"
		[uri]$githubRequestUri = "https://api.github.com/repos/alexed1/fr8company/statuses/" + $sourceVersion

		$githubResponse = try
		{
			$response = Invoke-RestMethod -Uri $githubRequestUri -headers $headersGitHub -Method Post -Body $message -ContentType 'application/json'
		}
		catch
		{
			Write-Host "Failed to update build status."
            Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
            Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription
			exit 1;
		}
	}	
}

$basicAuthVSO = ("{0}:{1}" -f $vso_username,$vso_password)
$basicAuthVSO = [System.Text.Encoding]::UTF8.GetBytes($basicAuthVSO)
$basicAuthVSO = [System.Convert]::ToBase64String($basicAuthVSO)
$headersVSO = @{Authorization=("Basic {0}" -f $basicAuthVSO)}

$basicAuthGitHub = ("{0}:{1}" -f $github_username,$github_password)
$basicAuthGitHub = [System.Text.Encoding]::UTF8.GetBytes($basicAuthGitHub)
$basicAuthGitHub = [System.Convert]::ToBase64String($basicAuthGitHub)
$headersGitHub = @{Authorization=("Basic {0}" -f $basicAuthGitHub)}

[uri]$vsoTimelineUri = "https://fr8.visualstudio.com/defaultcollection/fr8/_apis/build/builds/" + $buildId + "/timeline?api-version=2.0"
$vsoResponse = try
{
	Invoke-RestMethod -Uri $vsoTimelineUri -headers $headersVSO -Method Get -ContentType 'application/json'
}
catch
{
	Write-Host "Failed to get current build status"
    Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
    Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription
	exit 1
}

$tasks = $vsoResponse.records | where {$_.type -eq "Task"}
$succeededBuildSteps = $tasks | where {$_.name -notlike "Update GitHub status*" -and $_.state -eq "completed" -and $_.result -eq "succeeded" -and $_.state -eq 'Disabled'}

if ($succeededBuildSteps.Count -eq $tasks.Count - 1)
{
    UpdateGitHubBuildStatus -message $success
}
else
{
    UpdateGitHubBuildStatus -message $failure
}

Invoke-Expression "git checkout . 2> $tempFileName"
Invoke-Expression "git checkout $branchName 2> $tempFileName"
if ($LastExitCode -ne 0)
{
	Write-Host "Failed checkout to $branchName branch."
	exit 1
}
else
{
	$buildBranchName = $mainBranchName + "+" + $branchName
	DeleteBranchIfExists $buildBranchName
}

if (Test-Path $tempFileName) {
  Remove-Item $tempFileName
}

