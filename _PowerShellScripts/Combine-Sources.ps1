<#
    .SYNOPSIS
#>

param(
	[Parameter(Mandatory = $true)]
	[string]$github_username,

	[Parameter(Mandatory = $true)]
	[string]$github_password,

	[string]$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY,

    [string]$tempDirectory = $env:BUILD_STAGINGDIRECTORY
)

# $ErrorActionPreference = 'Stop'

$githubUrlBase = "https://{0}:{1}@github.com/Fr8org/" -f $github_username, $github_password

# Add more open source repos here
$openSourceRepos = @("terminalSlack")

# Append github URL to the repos
ForEach ($currentRepo in $openSourceRepos) { 
    Write-Host ("Processing {0}" -f $currentRepo)
    $currentRepoUrl = [System.IO.Path]::Combine($githubUrlBase, $currentRepo)
	$curTemp =[System.IO.Path]::Combine($tempDirectory, $currentRepo)
    $tempFileName = [System.IO.Path]::Combine($tempDirectory, $currentRepo + "_log.txt")

	if (Test-Path $curTemp) {
        cd $curTemp
		Remove-Item * -Force -Recurse
	}
    else {
	    md $curTemp
    	cd $curTemp
    }

	# Fetch open source repo
	$command = "git clone $currentRepoUrl $curTemp 2> $tempFileName"
    Invoke-Expression  $command

	if ($LastExitCode -ne 0)
	{
		Write-Host ("Failed to clone the repository {0}." -f $currentRepo)
		exit 1;
	}

    $curTerminal = [System.IO.Path]::Combine($sourcesDirectory, $currentRepo)

    if (Test-Path $curTerminal) {
        # Delete terminal from the main working directory (if exists)
        Remove-Item $curTerminal -Force -Recurse
    }

    # Re-create terminal subdirectory
    md $curTerminal

	# Move cloned directory with files to the main working directory (exclude .git folder)
    Robocopy $curTemp $curTerminal /S /XD .git
} 